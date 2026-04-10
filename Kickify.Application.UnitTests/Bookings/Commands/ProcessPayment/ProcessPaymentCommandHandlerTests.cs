using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
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
using Xunit;

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


    
    // Covers UTCID32 from CSV
    [Fact]
    public async Task Handle_ProcessPayment_WhenAllPaid_ShouldConfirmBookingAndLockRoom_UTCID32()
    {
        // Arrange
        var command = new ProcessPaymentCommand(Guid.NewGuid());
        var userId = Guid.NewGuid();
        var user = new User { UserId = userId, FullName = "Test User" };
        var roomId = command.RoomId;

        _userContextMock.Setup(uc => uc.UserId).Returns(userId);
        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);

        var room = new MatchRoom
        {
            RoomId = roomId,
            Status = RoomStatus.Open,
            DepositPerPerson = 100,
            FieldId = Guid.NewGuid(),
            FilledSlots = 2,
            TotalSlots = 2,
            MatchDate = DateTime.Today,
            StartTime = new TimeSpan(10, 0, 0),
            RoomParticipants = new List<RoomParticipant>
            {
                new RoomParticipant { UserId = userId, DepositPaid = false },
                new RoomParticipant { UserId = Guid.NewGuid(), DepositPaid = true, DepositAmount = 100 }
            }
        };

        _matchRoomRepositoryMock.Setup(repo => repo.GetRoomWithParticipantsForUpdateAsync(roomId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(room);

        var wallet = new Wallet { WalletId = Guid.NewGuid(), UserId = userId, Balance = 200 };
        _walletRepositoryMock.Setup(repo => repo.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(wallet);

        var field = new Field { FieldId = room.FieldId.Value };
        _fieldRepositoryMock.Setup(repo => repo.GetFieldWithVenueAsync(room.FieldId.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(field);

        var booking = new Booking { BookingId = Guid.NewGuid(), BookingDate = DateTime.Today, StartTime = new TimeSpan(10, 0, 0), EndTime = new TimeSpan(11, 0, 0) };
        _bookingRepositoryMock.Setup(repo => repo.GetBookingByRoomAsync(roomId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None); // Changed _handler to _sut

        // Assert
        result.IsSuccess.Should().BeTrue();
        room.Status.Should().Be(RoomStatus.Locked);
        booking.Status.Should().Be(BookingStatus.Confirmed);

        _walletTransactionRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<WalletTransaction>()), Times.Once);
        _matchRoomRepositoryMock.Verify(repo => repo.Update(It.IsAny<MatchRoom>()), Times.Once);
        _bookingRepositoryMock.Verify(repo => repo.Update(It.IsAny<Booking>()), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        _matchRoomHubServiceMock.Verify(hub => hub.NotifyRoomStatusChangedAsync(roomId, RoomStatus.Locked.ToString(), It.IsAny<CancellationToken>()), Times.Once);
    }

    // Covers UTCID33 from CSV
    [Fact]
    public async Task Handle_ProcessPayment_WhenAlreadyPaid_ShouldReturnError_UTCID33()
    {
        // Arrange
        var command = new ProcessPaymentCommand(Guid.NewGuid());
        var userId = Guid.NewGuid();
        var user = new User { UserId = userId };

        _userContextMock.Setup(uc => uc.UserId).Returns(userId);
        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);

        var room = new MatchRoom
        {
            RoomId = command.RoomId,
            RoomParticipants = new List<RoomParticipant>
            {
                new RoomParticipant { UserId = userId, DepositPaid = true }
            }
        };

        _matchRoomRepositoryMock.Setup(repo => repo.GetRoomWithParticipantsForUpdateAsync(command.RoomId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(room);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None); // Changed _handler to _sut

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(BookingErrors.AlreadyPaid);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // Covers UTCID34 from CSV
    [Fact]
    public async Task Handle_ProcessPayment_WhenInsufficientBalance_ShouldReturnError_UTCID34()
    {
        // Arrange
        var command = new ProcessPaymentCommand(Guid.NewGuid());
        var userId = Guid.NewGuid();
        var user = new User { UserId = userId };

        _userContextMock.Setup(uc => uc.UserId).Returns(userId);
        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);

        var room = new MatchRoom
        {
            RoomId = command.RoomId,
            DepositPerPerson = 100,
            RoomParticipants = new List<RoomParticipant>
            {
                new RoomParticipant { UserId = userId, DepositPaid = false }
            }
        };

        _matchRoomRepositoryMock.Setup(repo => repo.GetRoomWithParticipantsForUpdateAsync(command.RoomId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(room);

        var wallet = new Wallet { WalletId = Guid.NewGuid(), UserId = userId, Balance = 50 }; // Less than deposit (100)
        _walletRepositoryMock.Setup(repo => repo.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(wallet);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None); // Changed _handler to _sut

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(WalletErrors.InsufficientBalance);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // Covers UTCID35 from CSV
    [Fact]
    public async Task Handle_ProcessPayment_WhenParticipantNotFound_ShouldReturnError_UTCID35()
    {
        // Arrange
        var command = new ProcessPaymentCommand(Guid.NewGuid());
        var userId = Guid.NewGuid();
        var user = new User { UserId = userId };

        _userContextMock.Setup(uc => uc.UserId).Returns(userId);
        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);

        var room = new MatchRoom
        {
            RoomId = command.RoomId,
            RoomParticipants = new List<RoomParticipant>() // Empty, so user is not participant
        };

        _matchRoomRepositoryMock.Setup(repo => repo.GetRoomWithParticipantsForUpdateAsync(command.RoomId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(room);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None); // Changed _handler to _sut

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(BookingErrors.ParticipantNotFound);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}