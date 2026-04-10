using FluentAssertions;
using Hangfire;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Abstractions.Services;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Domain.Event;
using Kickify.Infrastructure.Jobs;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace Kickify.Application.UnitTests.Jobs;

public class RoomAutoCloseServiceTests
{
    private readonly Mock<IBackgroundJobClient> _backgroundJobClientMock;
    private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
    private readonly Mock<IServiceScope> _serviceScopeMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<ILogger<RoomAutoCloseService>> _loggerMock;

    private readonly Mock<IMatchRoomRepository> _matchRoomRepositoryMock;
    private readonly Mock<IBookingRepository> _bookingRepositoryMock;
    private readonly Mock<IRoomParticipantRepository> _participantRepositoryMock;
    private readonly Mock<IWalletRepository> _walletRepositoryMock;
    private readonly Mock<IWalletTransactionRepository> _walletTransactionRepositoryMock;
    private readonly Mock<IMatchRoomHubService> _matchRoomHubServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IPublisher> _publisherMock;

    private readonly RoomAutoCloseService _service;

    public RoomAutoCloseServiceTests()
    {
        _backgroundJobClientMock = new Mock<IBackgroundJobClient>();
        _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        _serviceScopeMock = new Mock<IServiceScope>();
        _serviceProviderMock = new Mock<IServiceProvider>();
        _loggerMock = new Mock<ILogger<RoomAutoCloseService>>();

        _matchRoomRepositoryMock = new Mock<IMatchRoomRepository>();
        _bookingRepositoryMock = new Mock<IBookingRepository>();
        _participantRepositoryMock = new Mock<IRoomParticipantRepository>();
        _walletRepositoryMock = new Mock<IWalletRepository>();
        _walletTransactionRepositoryMock = new Mock<IWalletTransactionRepository>();
        _matchRoomHubServiceMock = new Mock<IMatchRoomHubService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _publisherMock = new Mock<IPublisher>();

        _serviceScopeFactoryMock.Setup(x => x.CreateScope()).Returns(_serviceScopeMock.Object);
        _serviceScopeMock.Setup(x => x.ServiceProvider).Returns(_serviceProviderMock.Object);

        _serviceProviderMock.Setup(x => x.GetService(typeof(IMatchRoomRepository))).Returns(_matchRoomRepositoryMock.Object);
        _serviceProviderMock.Setup(x => x.GetService(typeof(IBookingRepository))).Returns(_bookingRepositoryMock.Object);
        _serviceProviderMock.Setup(x => x.GetService(typeof(IRoomParticipantRepository))).Returns(_participantRepositoryMock.Object);
        _serviceProviderMock.Setup(x => x.GetService(typeof(IWalletRepository))).Returns(_walletRepositoryMock.Object);
        _serviceProviderMock.Setup(x => x.GetService(typeof(IWalletTransactionRepository))).Returns(_walletTransactionRepositoryMock.Object);
        _serviceProviderMock.Setup(x => x.GetService(typeof(IMatchRoomHubService))).Returns(_matchRoomHubServiceMock.Object);
        _serviceProviderMock.Setup(x => x.GetService(typeof(IUnitOfWork))).Returns(_unitOfWorkMock.Object);
        _serviceProviderMock.Setup(x => x.GetService(typeof(IPublisher))).Returns(_publisherMock.Object);

        _service = new RoomAutoCloseService(
            _backgroundJobClientMock.Object,
            _serviceScopeFactoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task CloseRoomAsync_WhenRoomIsNotOpen_ShouldDoNothing()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        var room = new MatchRoom { RoomId = roomId, Status = RoomStatus.Locked };
        _matchRoomRepositoryMock.Setup(r => r.GetByIdAsync(roomId)).ReturnsAsync(room);

        // Act
        await _service.CloseRoomAsync(roomId);

        // Assert
        room.Status.Should().Be(RoomStatus.Locked);
        _matchRoomRepositoryMock.Verify(r => r.Update(It.IsAny<MatchRoom>()), Times.Never);
    }

    [Fact]
    public async Task CloseRoomAsync_WhenRoomIsOpen_ShouldCancelAndRefund()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        var creatorId = Guid.NewGuid();
        var memberId = Guid.NewGuid();

        var room = new MatchRoom
        {
            RoomId = roomId,
            Status = RoomStatus.Open,
            HostId = creatorId
        };

        var participants = new List<RoomParticipant>
        {
            new RoomParticipant { UserId = memberId, DepositPaid = true, DepositAmount = 50 },
            new RoomParticipant { UserId = creatorId, DepositPaid = false, DepositAmount = 0 }
        };

        var booking = new Booking { BookingId = Guid.NewGuid(), Status = BookingStatus.Pending };

        var memberWallet = new Wallet { WalletId = Guid.NewGuid(), UserId = memberId, Balance = 200 };

        _matchRoomRepositoryMock.Setup(r => r.GetByIdAsync(roomId)).ReturnsAsync(room);
        _participantRepositoryMock.Setup(r => r.GetParticipantsByRoomAsync(roomId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(participants);
        _bookingRepositoryMock.Setup(r => r.GetBookingByRoomAsync(roomId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);
        _walletRepositoryMock.Setup(r => r.GetByUserIdAsync(memberId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(memberWallet);

        // Act
        await _service.CloseRoomAsync(roomId);

        // Assert
        room.Status.Should().Be(RoomStatus.Cancelled);
        booking.Status.Should().Be(BookingStatus.Cancelled);
        memberWallet.Balance.Should().Be(250);

        _matchRoomRepositoryMock.Verify(r => r.Update(room), Times.Once);
        _bookingRepositoryMock.Verify(r => r.Update(booking), Times.Once);
        _walletRepositoryMock.Verify(r => r.Update(memberWallet), Times.Once);
        _walletTransactionRepositoryMock.Verify(r => r.AddAsync(It.IsAny<WalletTransaction>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _publisherMock.Verify(m => m.Publish(It.IsAny<MatchRoomCancelledNotifyParticipantsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        _matchRoomHubServiceMock.Verify(m => m.NotifyRoomStatusChangedAsync(roomId, RoomStatus.Cancelled.ToString(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
