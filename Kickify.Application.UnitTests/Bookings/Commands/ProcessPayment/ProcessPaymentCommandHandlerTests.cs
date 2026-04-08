using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Jobs;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Abstractions.Services;
using Kickify.Application.Features.Bookings.Commands.ProcessPayment;
using Kickify.Domain.Common;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace Kickify.Application.UnitTests.Bookings.Commands.ProcessPayment;

public class ProcessPaymentCommandHandlerTests
{
    private readonly Mock<IMatchRoomRepository> _matchRoomRepositoryMock = new();
    private readonly Mock<IRoomParticipantRepository> _roomParticipantRepositoryMock = new();
    private readonly Mock<IBookingRepository> _bookingRepositoryMock = new();
    private readonly Mock<IFieldRepository> _fieldRepositoryMock = new();
    private readonly Mock<IVenueRepository> _venueRepositoryMock = new();
    private readonly Mock<IWalletRepository> _walletRepositoryMock = new();
    private readonly Mock<IWalletTransactionRepository> _walletTransactionRepositoryMock = new();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IMatchRoomHubService> _matchRoomHubServiceMock = new();
    private readonly Mock<IMatchLifecycleService> _matchLifecycleServiceMock = new();
    private readonly Mock<IRoomAutoCloseService> _roomAutoCloseServiceMock = new();
    private readonly Mock<IUserContext> _userContextMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock = new();
    private readonly Mock<ILogger<ProcessPaymentCommandHandler>> _loggerMock = new();

    private readonly ProcessPaymentCommandHandler _sut;

    public ProcessPaymentCommandHandlerTests()
    {
        _sut = new ProcessPaymentCommandHandler(
            _matchRoomRepositoryMock.Object,
            _roomParticipantRepositoryMock.Object,
            _bookingRepositoryMock.Object,
            _fieldRepositoryMock.Object,
            _venueRepositoryMock.Object,
            _walletRepositoryMock.Object,
            _walletTransactionRepositoryMock.Object,
            _userRepositoryMock.Object,
            _matchRoomHubServiceMock.Object,
            _matchLifecycleServiceMock.Object,
            _roomAutoCloseServiceMock.Object,
            _userContextMock.Object,
            _unitOfWorkMock.Object,
            _serviceScopeFactoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WhenCurrentUserNotFound_ReturnsUserNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        _userContextMock.SetupGet(x => x.UserId).Returns(userId);

        var command = new ProcessPaymentCommand(roomId);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(UserErrors.NotFound(userId).Code);
    }

    [Fact]
    public async Task Handle_WhenRoomNotFound_ReturnsRoomNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        _userContextMock.SetupGet(x => x.UserId).Returns(userId);

        var command = new ProcessPaymentCommand(roomId);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(new User { UserId = userId });

        _matchRoomRepositoryMock
            .Setup(x => x.GetRoomWithParticipantsForUpdateAsync(roomId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MatchRoom?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(BookingErrors.RoomNotFound(roomId).Code);
    }

    [Fact]
    public async Task Handle_WhenUserNotParticipant_ReturnsParticipantNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        _userContextMock.SetupGet(x => x.UserId).Returns(userId);

        var command = new ProcessPaymentCommand(roomId);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(new User { UserId = userId });

        var room = new MatchRoom
        {
            RoomId = roomId,
            RoomParticipants = new List<RoomParticipant>()
        };

        _matchRoomRepositoryMock
            .Setup(x => x.GetRoomWithParticipantsForUpdateAsync(roomId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(room);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(BookingErrors.ParticipantNotFound.Code);
    }

    [Fact]
    public async Task Handle_WhenAlreadyPaid_ReturnsAlreadyPaid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        _userContextMock.SetupGet(x => x.UserId).Returns(userId);

        var command = new ProcessPaymentCommand(roomId);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(new User { UserId = userId });

        var participant = new RoomParticipant
        {
            UserId = userId,
            DepositPaid = true
        };

        var room = new MatchRoom
        {
            RoomId = roomId,
            RoomParticipants = new List<RoomParticipant> { participant }
        };

        _matchRoomRepositoryMock
            .Setup(x => x.GetRoomWithParticipantsForUpdateAsync(roomId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(room);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(BookingErrors.AlreadyPaid.Code);
    }

    [Fact]
    public async Task Handle_WhenWalletNotFound_ReturnsWalletNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        _userContextMock.SetupGet(x => x.UserId).Returns(userId);

        var command = new ProcessPaymentCommand(roomId);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(new User { UserId = userId });

        var participant = new RoomParticipant
        {
            UserId = userId,
            DepositPaid = false
        };

        var room = new MatchRoom
        {
            RoomId = roomId,
            DepositPerPerson = 50000,
            RoomParticipants = new List<RoomParticipant> { participant }
        };

        _matchRoomRepositoryMock
            .Setup(x => x.GetRoomWithParticipantsForUpdateAsync(roomId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(room);

        _walletRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Wallet?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(WalletErrors.WalletNotFound.Code);
    }

    [Fact]
    public async Task Handle_WhenInsufficientBalance_ReturnsInsufficientBalance()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        _userContextMock.SetupGet(x => x.UserId).Returns(userId);

        var command = new ProcessPaymentCommand(roomId);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(new User { UserId = userId });

        var participant = new RoomParticipant
        {
            UserId = userId,
            DepositPaid = false
        };

        var room = new MatchRoom
        {
            RoomId = roomId,
            DepositPerPerson = 50000,
            RoomParticipants = new List<RoomParticipant> { participant }
        };

        _matchRoomRepositoryMock
            .Setup(x => x.GetRoomWithParticipantsForUpdateAsync(roomId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(room);

        var wallet = new Wallet
        {
            WalletId = Guid.NewGuid(),
            UserId = userId,
            Balance = 10000
        };

        _walletRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(wallet);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(WalletErrors.InsufficientBalance.Code);
    }

    [Fact]
    public async Task ProcessPayment_WhenRoomIsNotLocked_DoesNotCancelAutoClose()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        _userContextMock.SetupGet(x => x.UserId).Returns(userId);

        var command = new ProcessPaymentCommand(roomId);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(new User { UserId = userId });

        var participant1 = new RoomParticipant
        {
            UserId = userId,
            DepositPaid = false
        };
        var participant2 = new RoomParticipant
        {
            UserId = Guid.NewGuid(),
            DepositPaid = false
        };

        var room = new MatchRoom
        {
            RoomId = roomId,
            DepositPerPerson = 50000,
            FilledSlots = 2,
            TotalSlots = 2,
            RoomParticipants = new List<RoomParticipant> { participant1, participant2 },
            AutoCloseJobId = "job-123",
            FieldId = Guid.NewGuid()
        };

        _matchRoomRepositoryMock
            .Setup(x => x.GetRoomWithParticipantsForUpdateAsync(roomId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(room);

        var wallet = new Wallet
        {
            WalletId = Guid.NewGuid(),
            UserId = userId,
            Balance = 100000
        };

        _walletRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(wallet);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _roomAutoCloseServiceMock.Verify(x => x.CancelAutoClose(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ProcessPayment_WhenRoomIsLocked_CancelsAutoClose()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        _userContextMock.SetupGet(x => x.UserId).Returns(userId);

        var command = new ProcessPaymentCommand(roomId);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(new User { UserId = userId });

        var participant1 = new RoomParticipant
        {
            UserId = userId,
            DepositPaid = false
        };
        var participant2 = new RoomParticipant
        {
            UserId = Guid.NewGuid(),
            DepositPaid = true
        };

        var room = new MatchRoom
        {
            RoomId = roomId,
            DepositPerPerson = 50000,
            FilledSlots = 2,
            TotalSlots = 2,
            RoomParticipants = new List<RoomParticipant> { participant1, participant2 },
            AutoCloseJobId = "job-123",
            FieldId = Guid.NewGuid(),
            MatchDate = DateTime.Today,
            StartTime = new TimeSpan(10, 0, 0)
        };

        _matchRoomRepositoryMock
            .Setup(x => x.GetRoomWithParticipantsForUpdateAsync(roomId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(room);

        var wallet = new Wallet
        {
            WalletId = Guid.NewGuid(),
            UserId = userId,
            Balance = 100000
        };

        _walletRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(wallet);

        var field = new Field { FieldId = room.FieldId.Value, VenueId = Guid.NewGuid() };
        _fieldRepositoryMock
            .Setup(x => x.GetFieldWithVenueAsync(room.FieldId.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(field);

        var booking = new Booking { BookingId = Guid.NewGuid(), RoomId = roomId };
        _bookingRepositoryMock
            .Setup(x => x.GetBookingByRoomAsync(roomId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _roomAutoCloseServiceMock.Verify(x => x.CancelAutoClose(room.AutoCloseJobId), Times.Once);
    }
}

