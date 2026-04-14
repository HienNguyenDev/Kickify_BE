using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Abstractions.Services;
using Kickify.Application.Features.MatchRooms.Commands.JoinRoom;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;
using Kickify.Domain.Event;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Kickify.Application.UnitTests.MatchRooms.Commands.JoinRoom;

public class JoinRoomCommandHandlerTests
{
    private readonly Mock<IMatchRoomRepository> _matchRoomRepositoryMock = new();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IRoomParticipantRepository> _roomParticipantRepositoryMock = new();
    private readonly Mock<IMatchRoomHubService> _matchRoomHubServiceMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IUserContext> _userContextMock = new();
    private readonly Mock<IPublisher> _publisherMock = new();
    private readonly Mock<ILogger<JoinRoomCommandHandler>> _loggerMock = new();

    private readonly JoinRoomCommandHandler _sut;

    public JoinRoomCommandHandlerTests()
    {
        _matchRoomRepositoryMock
            .Setup(x => x.GetActiveRoomsForUserByDateAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MatchRoom>());

        _sut = new JoinRoomCommandHandler(
            _matchRoomRepositoryMock.Object,
            _userRepositoryMock.Object,
            _roomParticipantRepositoryMock.Object,
            _matchRoomHubServiceMock.Object,
            _unitOfWorkMock.Object,
            _userContextMock.Object,
            _publisherMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ReturnsUserNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        _userContextMock.SetupGet(x => x.UserId).Returns(userId);

        var command = new JoinRoomCommand(roomId, null);

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
    public async Task Handle_WhenAlreadyParticipant_ReturnsExistingRoomInfo()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        _userContextMock.SetupGet(x => x.UserId).Returns(userId);

        var command = new JoinRoomCommand(roomId, null);

        var user = new User { UserId = userId, Email = "user@example.com" };
        var participant = new RoomParticipant
        {
            ParticipantId = Guid.NewGuid(),
            RoomId = roomId,
            UserId = userId,
            JoinDate = DateTime.UtcNow.AddMinutes(-5)
        };
        var room = new MatchRoom
        {
            RoomId = roomId,
            FilledSlots = 3,
            TotalSlots = 10
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);

        _roomParticipantRepositoryMock
            .Setup(x => x.GetParticipantByRoomAndUserAsync(roomId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(participant);

        _matchRoomRepositoryMock
            .Setup(x => x.GetByIdAsync(roomId))
            .ReturnsAsync(room);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.ParticipantId.Should().Be(participant.ParticipantId);
        result.Value.RoomId.Should().Be(roomId);
        result.Value.UserId.Should().Be(userId);
        result.Value.FilledSlots.Should().Be(room.FilledSlots);
        result.Value.TotalSlots.Should().Be(room.TotalSlots);
        result.Value.JoinDate.Should().Be(participant.JoinDate);
    }

    [Fact]
    public async Task Handle_WhenRoomNotFound_ReturnsMatchRoomNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        _userContextMock.SetupGet(x => x.UserId).Returns(userId);

        var command = new JoinRoomCommand(roomId, null);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(new User { UserId = userId });

        _roomParticipantRepositoryMock
            .Setup(x => x.GetParticipantByRoomAndUserAsync(roomId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((RoomParticipant?)null);

        _matchRoomRepositoryMock
            .Setup(x => x.GetRoomWithParticipantsForUpdateAsync(roomId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MatchRoom?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(MatchRoomErrors.NotFound(roomId).Code);
    }

    [Fact]
    public async Task Handle_WhenRoomNotOpen_ReturnsNotOpen()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        _userContextMock.SetupGet(x => x.UserId).Returns(userId);

        var command = new JoinRoomCommand(roomId, null);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(new User { UserId = userId });

        _roomParticipantRepositoryMock
            .Setup(x => x.GetParticipantByRoomAndUserAsync(roomId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((RoomParticipant?)null);

        var room = new MatchRoom
        {
            RoomId = roomId,
            Status = RoomStatus.Locked
        };

        _matchRoomRepositoryMock
            .Setup(x => x.GetRoomWithParticipantsForUpdateAsync(roomId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(room);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(MatchRoomErrors.NotOpen.Code);
    }

    [Fact]
    public async Task Handle_PrivateRoomWithoutPassword_ReturnsPasswordRequired()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        _userContextMock.SetupGet(x => x.UserId).Returns(userId);

        var command = new JoinRoomCommand(roomId, null);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(new User { UserId = userId });

        _roomParticipantRepositoryMock
            .Setup(x => x.GetParticipantByRoomAndUserAsync(roomId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((RoomParticipant?)null);

        var room = new MatchRoom
        {
            RoomId = roomId,
            Status = RoomStatus.Open,
            Visibility = Visibility.Private
        };

        _matchRoomRepositoryMock
            .Setup(x => x.GetRoomWithParticipantsForUpdateAsync(roomId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(room);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be(MatchRoomErrors.PasswordRequiredForPrivateRoom.Code);
    }

    [Fact]
    public async Task Handle_WhenPlayerJoins_DoesNotTriggerAutoCloseExtensionEvent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        _userContextMock.SetupGet(x => x.UserId).Returns(userId);

        var command = new JoinRoomCommand(roomId, null);

        var user = new User { UserId = userId, FullName = "Test User", Email = "test@example.com" };
        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Player not yet in the room
        _roomParticipantRepositoryMock
            .Setup(x => x.GetParticipantByRoomAndUserAsync(roomId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((RoomParticipant?)null);

        var room = new MatchRoom
        {
            RoomId = roomId,
            Status = RoomStatus.Open,
            Visibility = Visibility.Public,
            FilledSlots = 1,
            TotalSlots = 10,
            DepositPerPerson = 50000,
            AutoCloseJobId = "job-123"
        };
        // Ensure starting clean
        room.ClearDomainEvents();

        _matchRoomRepositoryMock
            .Setup(x => x.GetRoomWithParticipantsForUpdateAsync(roomId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(room);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify no ParticipantJoinedRoomDomainEvent was raised
        var domainEvents = room.DomainEvents;
        domainEvents.OfType<ParticipantJoinedRoomDomainEvent>().Should().BeEmpty("Because player joining should NOT extend the auto-close timer by 20 minutes anymore.");
    }

    // =========================================================================
    // NEW TESTS
    // =========================================================================

    [Fact]
    public async Task Handle_AlreadyInRoom_ReturnsSuccessIdempotent_UTCID11()
    {
        var userId = Guid.NewGuid(); var roomId = Guid.NewGuid();
        _userContextMock.Setup(x => x.UserId).Returns(userId);
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(new User { UserId = userId });

        var participant = new RoomParticipant { ParticipantId = Guid.NewGuid(), JoinDate = DateTime.UtcNow };
        _roomParticipantRepositoryMock.Setup(x => x.GetParticipantByRoomAndUserAsync(roomId, userId, It.IsAny<CancellationToken>())).ReturnsAsync(participant);
        _matchRoomRepositoryMock.Setup(x => x.GetByIdAsync(roomId)).ReturnsAsync(new MatchRoom { RoomId = roomId, FilledSlots = 1, TotalSlots = 10 });

        var result = await _sut.Handle(new JoinRoomCommand(roomId, null), CancellationToken.None);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_StatusNotOpen_ReturnsFailure_UTCID12()
    {
        var userId = Guid.NewGuid(); var roomId = Guid.NewGuid();
        _userContextMock.Setup(x => x.UserId).Returns(userId);
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(new User { UserId = userId });
        _matchRoomRepositoryMock.Setup(x => x.GetRoomWithParticipantsForUpdateAsync(roomId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MatchRoom { RoomId = roomId, Status = RoomStatus.Locked });

        var result = await _sut.Handle(new JoinRoomCommand(roomId, null), CancellationToken.None);
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be(MatchRoomErrors.NotOpen.Code);
    }

    [Fact]
    public async Task Handle_TimeConflict_ReturnsFailure_UTCID13()
    {
        var userId = Guid.NewGuid(); var roomId = Guid.NewGuid();
        _userContextMock.Setup(x => x.UserId).Returns(userId);
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(new User { UserId = userId });
        var roomDate = DateTime.UtcNow.Date;
        _matchRoomRepositoryMock.Setup(x => x.GetRoomWithParticipantsForUpdateAsync(roomId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MatchRoom { RoomId = roomId, Status = RoomStatus.Open, MatchDate = roomDate, StartTime = TimeSpan.FromHours(18), DurationMinutes = 60 });
        _matchRoomRepositoryMock.Setup(x => x.GetActiveRoomsForUserByDateAsync(userId, roomDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MatchRoom> { new MatchRoom { RoomId = Guid.NewGuid(), MatchDate = roomDate, StartTime = TimeSpan.FromHours(18), DurationMinutes = 60, RoomName = "Other Room" } });

        var result = await _sut.Handle(new JoinRoomCommand(roomId, null), CancellationToken.None);
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be(MatchRoomErrors.TimeConflict("Other Room").Code);
    }

    [Fact]
    public async Task Handle_RoomFull_ReturnsFailure_UTCID14()
    {
        var userId = Guid.NewGuid(); var roomId = Guid.NewGuid();
        _userContextMock.Setup(x => x.UserId).Returns(userId);
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(new User { UserId = userId });
        _matchRoomRepositoryMock.Setup(x => x.GetRoomWithParticipantsForUpdateAsync(roomId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MatchRoom { RoomId = roomId, Status = RoomStatus.Open, FilledSlots = 10, TotalSlots = 10, Visibility = Visibility.Public });

        var result = await _sut.Handle(new JoinRoomCommand(roomId, null), CancellationToken.None);
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be(MatchRoomErrors.RoomFull.Code);
    }

    [Fact]
    public async Task Handle_PrivateRoomWrongPassword_ReturnsFailure_UTCID15()
    {
        var userId = Guid.NewGuid(); var roomId = Guid.NewGuid();
        _userContextMock.Setup(x => x.UserId).Returns(userId);
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(new User { UserId = userId });
        _matchRoomRepositoryMock.Setup(x => x.GetRoomWithParticipantsForUpdateAsync(roomId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MatchRoom { RoomId = roomId, Status = RoomStatus.Open, FilledSlots = 1, TotalSlots = 10, Visibility = Visibility.Private, RoomPassword = "123" });

        var result = await _sut.Handle(new JoinRoomCommand(roomId, "wrong"), CancellationToken.None);
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be(MatchRoomErrors.IncorrectRoomPassword.Code);
    }

    [Fact]
    public async Task Handle_OpenAndFree_ReturnsSuccess_UTCID16()
    {
        var userId = Guid.NewGuid(); var roomId = Guid.NewGuid();
        _userContextMock.Setup(x => x.UserId).Returns(userId);
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(new User { UserId = userId, FullName = "Player 1" });
        _matchRoomRepositoryMock.Setup(x => x.GetRoomWithParticipantsForUpdateAsync(roomId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MatchRoom { RoomId = roomId, Status = RoomStatus.Open, FilledSlots = 1, TotalSlots = 10, Visibility = Visibility.Public, HostId = Guid.NewGuid() });

        var result = await _sut.Handle(new JoinRoomCommand(roomId, null), CancellationToken.None);
        result.IsSuccess.Should().BeTrue();
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_UserNotFound_ReturnsFailure_UTCID_New()
    {
        var userId = Guid.NewGuid(); var roomId = Guid.NewGuid();
        _userContextMock.SetupGet(x => x.UserId).Returns(userId);
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync((User?)null);

        var result = await _sut.Handle(new JoinRoomCommand(roomId, null), CancellationToken.None);
        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be(UserErrors.NotFound(userId).Code);
    }
}