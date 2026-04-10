using FluentAssertions;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Abstractions.Services;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Infrastructure.Jobs;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace Kickify.Application.UnitTests.Jobs;

public class MatchLifecycleServiceTests
{
    private readonly Mock<IBackgroundJobClient> _backgroundJobClientMock;
    private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
    private readonly Mock<IServiceScope> _serviceScopeMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<ILogger<MatchLifecycleService>> _loggerMock;

    private readonly Mock<IMatchRoomRepository> _matchRoomRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IBookingRepository> _bookingRepositoryMock;
    private readonly Mock<IFieldRepository> _fieldRepositoryMock;
    private readonly Mock<IVenueRepository> _venueRepositoryMock;
    private readonly Mock<IWalletRepository> _walletRepositoryMock;
    private readonly Mock<IWalletTransactionRepository> _walletTransactionRepositoryMock;
    private readonly Mock<IMatchRoomHubService> _matchRoomHubServiceMock;
    private readonly Mock<IPublisher> _publisherMock;

    private readonly MatchLifecycleService _service;

    public MatchLifecycleServiceTests()
    {
        _backgroundJobClientMock = new Mock<IBackgroundJobClient>();
        _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        _serviceScopeMock = new Mock<IServiceScope>();
        _serviceProviderMock = new Mock<IServiceProvider>();
        _loggerMock = new Mock<ILogger<MatchLifecycleService>>();

        _matchRoomRepositoryMock = new Mock<IMatchRoomRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _bookingRepositoryMock = new Mock<IBookingRepository>();
        _fieldRepositoryMock = new Mock<IFieldRepository>();
        _venueRepositoryMock = new Mock<IVenueRepository>();
        _walletRepositoryMock = new Mock<IWalletRepository>();
        _walletTransactionRepositoryMock = new Mock<IWalletTransactionRepository>();
        _matchRoomHubServiceMock = new Mock<IMatchRoomHubService>();
        _publisherMock = new Mock<IPublisher>();

        _serviceScopeFactoryMock.Setup(x => x.CreateScope()).Returns(_serviceScopeMock.Object);
        _serviceScopeMock.Setup(x => x.ServiceProvider).Returns(_serviceProviderMock.Object);

        _serviceProviderMock.Setup(x => x.GetService(typeof(IMatchRoomRepository))).Returns(_matchRoomRepositoryMock.Object);
        _serviceProviderMock.Setup(x => x.GetService(typeof(IUnitOfWork))).Returns(_unitOfWorkMock.Object);
        _serviceProviderMock.Setup(x => x.GetService(typeof(IBookingRepository))).Returns(_bookingRepositoryMock.Object);
        _serviceProviderMock.Setup(x => x.GetService(typeof(IFieldRepository))).Returns(_fieldRepositoryMock.Object);
        _serviceProviderMock.Setup(x => x.GetService(typeof(IVenueRepository))).Returns(_venueRepositoryMock.Object);
        _serviceProviderMock.Setup(x => x.GetService(typeof(IWalletRepository))).Returns(_walletRepositoryMock.Object);
        _serviceProviderMock.Setup(x => x.GetService(typeof(IWalletTransactionRepository))).Returns(_walletTransactionRepositoryMock.Object);
        _serviceProviderMock.Setup(x => x.GetService(typeof(IMatchRoomHubService))).Returns(_matchRoomHubServiceMock.Object);
        _serviceProviderMock.Setup(x => x.GetService(typeof(IPublisher))).Returns(_publisherMock.Object);

        _service = new MatchLifecycleService(
            _backgroundJobClientMock.Object,
            _serviceScopeFactoryMock.Object,
            _loggerMock.Object);
    }

    // Covers UTCID48
    [Fact]
    public async Task StartMatchAsync_WhenMatchStartOccurs_ShouldSignalInProgressAndEscrow_UTCID48()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        var room = new MatchRoom
        {
            RoomId = roomId,
            Status = RoomStatus.Locked,
            MatchDate = DateTime.Today,
            StartTime = TimeSpan.FromHours(10),
            DurationMinutes = 60,
            FieldId = Guid.NewGuid(),
            TotalDepositCollected = 500
        };

        // Note: It's calling GetByIdAsync twice: once in StartMatchAsync, once in ScheduleMatchEnd (via UpdateRoomJobId)
        _matchRoomRepositoryMock.Setup(r => r.GetByIdAsync(roomId)).ReturnsAsync(room);

        // Required setup for TransferFundsToVenueOwnerAsync internal mechanism
        var field = new Field { FieldId = room.FieldId.Value, VenueId = Guid.NewGuid() };
        _fieldRepositoryMock.Setup(r => r.GetFieldWithVenueAsync(room.FieldId.Value, It.IsAny<CancellationToken>())).ReturnsAsync(field);
        var venue = new Venue { VenueId = field.VenueId, OwnerId = Guid.NewGuid() };
        _venueRepositoryMock.Setup(r => r.GetByIdAsync(venue.VenueId)).ReturnsAsync(venue);
        var wallet = new Wallet { WalletId = Guid.NewGuid(), UserId = venue.OwnerId, Balance = 1000 };
        _walletRepositoryMock.Setup(r => r.GetByUserIdAsync(venue.OwnerId, It.IsAny<CancellationToken>())).ReturnsAsync(wallet);

        // Act
        await _service.StartMatchAsync(roomId);

        // Assert
        room.Status.Should().Be(RoomStatus.InProgress);
        wallet.Balance.Should().Be(1500); // Venue owner should get Escrow
        _walletTransactionRepositoryMock.Verify(r => r.AddAsync(It.IsAny<WalletTransaction>()), Times.Once);
        // It updates once for Status = InProgress, and once for EndMatchJobId
        _matchRoomRepositoryMock.Verify(r => r.Update(room), Times.Exactly(2));
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
        
        // We also check Hangfire schedules MatchEnd via ScheduleMatchEnd
        _backgroundJobClientMock.Verify(c => c.Create(It.IsAny<Job>(), It.IsAny<ScheduledState>()), Times.Once);
    }

    // Covers UTCID49
    [Fact]
    public void ScheduleMatchStart_ShouldCalculateDelayAndQueueHangfire_UTCID49()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        var expectedJobId = "mock-hangfire-job-id";
        
        // Simulating Hangfire scheduling queue. 
        _backgroundJobClientMock
            .Setup(c => c.Create(It.IsAny<Job>(), It.IsAny<EnqueuedState>()))
            .Returns(expectedJobId); 

        var mockDelay = DateTime.UtcNow.AddMinutes(60);
        var room = new MatchRoom { RoomId = roomId };
        _matchRoomRepositoryMock.Setup(r => r.GetByIdAsync(roomId)).ReturnsAsync(room);

        // Act
        Action act = () => _service.ScheduleMatchStart(roomId, mockDelay);

        // Assert
        act.Should().NotThrow();
        _backgroundJobClientMock.Verify(c => c.Create(
            It.IsAny<Job>(),
            It.IsAny<ScheduledState>()), Times.Once);

    }
}
