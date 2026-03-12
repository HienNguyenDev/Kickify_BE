using System.Threading;
using System.Threading.Tasks;
using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Abstractions.Services;
using Kickify.Application.Features.MatchRooms.Commands.JoinRoom;
using Kickify.Domain.Common;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace Kickify.Application.UnitTests.MatchRooms.Commands.JoinRoom;

public class JoinRoomCommandHandlerTests
{
    private readonly Mock<IMatchRoomRepository> _matchRoomRepositoryMock = new();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IRoomParticipantRepository> _roomParticipantRepositoryMock = new();
    private readonly Mock<IMatchRoomHubService> _matchRoomHubServiceMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IUserContext> _userContextMock = new();
    private readonly Mock<ILogger<JoinRoomCommandHandler>> _loggerMock = new();

    private readonly JoinRoomCommandHandler _sut;

    public JoinRoomCommandHandlerTests()
    {
        _sut = new JoinRoomCommandHandler(
            _matchRoomRepositoryMock.Object,
            _userRepositoryMock.Object,
            _roomParticipantRepositoryMock.Object,
            _matchRoomHubServiceMock.Object,
            _unitOfWorkMock.Object,
            _userContextMock.Object,
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
}

