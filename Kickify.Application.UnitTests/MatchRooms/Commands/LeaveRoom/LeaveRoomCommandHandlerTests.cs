using System.Threading;
using System.Threading.Tasks;
using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Abstractions.Services;
using Kickify.Application.Features.MatchRooms.Commands.LeaveRoom;
using Kickify.Domain.Common;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;
using Microsoft.Extensions.Logging;
using Moq;

namespace Kickify.Application.UnitTests.MatchRooms.Commands.LeaveRoom;

public class LeaveRoomCommandHandlerTests
{
    private readonly Mock<IMatchRoomRepository> _matchRoomRepositoryMock = new();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IRoomParticipantRepository> _roomParticipantRepositoryMock = new();
    private readonly Mock<IChatMessageRepository> _chatMessageRepositoryMock = new();
    private readonly Mock<IMatchRoomHubService> _matchRoomHubServiceMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IUserContext> _userContextMock = new();
    private readonly Mock<ILogger<LeaveRoomCommandHandler>> _loggerMock = new();

    private readonly LeaveRoomCommandHandler _sut;

    public LeaveRoomCommandHandlerTests()
    {
        _sut = new LeaveRoomCommandHandler(
            _matchRoomRepositoryMock.Object,
            _userRepositoryMock.Object,
            _roomParticipantRepositoryMock.Object,
            _chatMessageRepositoryMock.Object,
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

        var command = new LeaveRoomCommand(roomId);

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

        var command = new LeaveRoomCommand(roomId);

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
        result.Error!.Code.Should().Be(MatchRoomErrors.NotFound(roomId).Code);
    }

    [Fact]
    public async Task Handle_WhenUserNotParticipant_ReturnsNotParticipant()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        _userContextMock.SetupGet(x => x.UserId).Returns(userId);

        var command = new LeaveRoomCommand(roomId);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(new User { UserId = userId });

        var room = new MatchRoom
        {
            RoomId = roomId,
            HostId = Guid.NewGuid(),
            FilledSlots = 1,
            TotalSlots = 5,
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
        result.Error!.Code.Should().Be(MatchRoomErrors.NotParticipant.Code);
    }

    [Fact]
    public async Task Handle_WhenHostIsLastParticipant_DeletesRoom()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        _userContextMock.SetupGet(x => x.UserId).Returns(userId);

        var command = new LeaveRoomCommand(roomId);

        var user = new User { UserId = userId, Email = "host@example.com" };
        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);

        var participant = new RoomParticipant
        {
            ParticipantId = Guid.NewGuid(),
            RoomId = roomId,
            UserId = userId
        };

        var room = new MatchRoom
        {
            RoomId = roomId,
            HostId = userId,
            FilledSlots = 1,
            TotalSlots = 5,
            RoomParticipants = new List<RoomParticipant> { participant }
        };

        _matchRoomRepositoryMock
            .Setup(x => x.GetRoomWithParticipantsForUpdateAsync(roomId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(room);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.RoomId.Should().Be(roomId);
        result.Value.UserId.Should().Be(userId);

        _matchRoomRepositoryMock.Verify(x => x.Remove(room), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _matchRoomHubServiceMock.Verify(
            x => x.NotifyUserLeftAsync(roomId, userId, It.IsAny<string>(), It.IsAny<int>(), room.TotalSlots, true, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}

