using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Abstractions.Services;
using Kickify.Application.Features.MatchRooms.Commands.UpdateParticipant;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Kickify.Application.UnitTests.MatchRooms.Commands.UpdateParticipant;

public class UpdateParticipantCommandHandlerTests
{
    private readonly Mock<IMatchRoomRepository> _matchRoomRepoMock = new();
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IRoomParticipantRepository> _participantRepoMock = new();
    private readonly Mock<IMatchRoomHubService> _hubServiceMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IUserContext> _userContextMock = new();
    private readonly Mock<ILogger<UpdateParticipantCommandHandler>> _loggerMock = new();

    private readonly UpdateParticipantCommandHandler _sut;

    public UpdateParticipantCommandHandlerTests()
    {
        _sut = new UpdateParticipantCommandHandler(
            _matchRoomRepoMock.Object,
            _userRepoMock.Object,
            _participantRepoMock.Object,
            _hubServiceMock.Object,
            _unitOfWorkMock.Object,
            _userContextMock.Object,
            _loggerMock.Object);
    }

    // Covers UTCID01 from CSV
    [Fact]
    public async Task Handle_UserNotInRoom_ReturnsNotParticipantError_UTCID01()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roomId = Guid.NewGuid();

        _userContextMock.Setup(x => x.UserId).Returns(userId);
        _userRepoMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(new User { UserId = userId });

        var room = new MatchRoom
        {
            RoomId = roomId,
            HostId = Guid.NewGuid(),
            Status = RoomStatus.Open,
            RoomParticipants = new List<RoomParticipant>() // Empty, so user is not in room
        };

        _matchRoomRepoMock.Setup(x => x.GetRoomWithParticipantsForUpdateAsync(roomId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(room);

        var command = new UpdateParticipantCommand(roomId, "A", null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be(MatchRoomErrors.NotParticipant.Code);
    }

    // Covers UTCID02 from CSV
    [Fact]
    public async Task Handle_InvalidTeamString_ReturnsInvalidTeamError_UTCID02()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roomId = Guid.NewGuid();

        _userContextMock.Setup(x => x.UserId).Returns(userId);
        _userRepoMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(new User { UserId = userId });

        var participant = new RoomParticipant { UserId = userId, RoomId = roomId };
        var room = new MatchRoom
        {
            RoomId = roomId,
            Status = RoomStatus.Open,
            RoomParticipants = new List<RoomParticipant> { participant }
        };

        _matchRoomRepoMock.Setup(x => x.GetRoomWithParticipantsForUpdateAsync(roomId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(room);

        var command = new UpdateParticipantCommand(roomId, "TeamC", null); // Invalid team string

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be(MatchRoomErrors.InvalidTeam("TeamC").Code);
    }

    // Covers UTCID03 from CSV
    [Fact]
    public async Task Handle_MoveToUnassigned_WasCaptainOfOldTeam_CaptainSuccessionTriggered_UTCID03()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        var newCaptainId = Guid.NewGuid();

        _userContextMock.Setup(x => x.UserId).Returns(userId);
        _userRepoMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(new User { UserId = userId, FullName = "Old Captain" });

        var participant = new RoomParticipant
        {
            UserId = userId,
            RoomId = roomId,
            TeamAssignment = TeamAssignment.A, // Old team
            IsCaptain = true // Was captain
        };

        var room = new MatchRoom
        {
            RoomId = roomId,
            Status = RoomStatus.Open,
            RoomParticipants = new List<RoomParticipant> { participant }
        };

        _matchRoomRepoMock.Setup(x => x.GetRoomWithParticipantsForUpdateAsync(roomId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(room);

        // Mock captain succession finding a new heir
        _participantRepoMock.Setup(x => x.AssignNewCaptainAsync(roomId, TeamAssignment.A, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(newCaptainId);

        var command = new UpdateParticipantCommand(roomId, "Unassigned", null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        participant.TeamAssignment.Should().Be(TeamAssignment.Unassigned);
        participant.IsCaptain.Should().BeFalse(); // Lost captaincy

        // Verify succession triggered
        _participantRepoMock.Verify(x => x.AssignNewCaptainAsync(roomId, TeamAssignment.A, userId, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // Covers UTCID04 from CSV
    [Fact]
    public async Task Handle_JoinATeam_NewTeamHasNoCaptain_BecameCaptainOfNewTeam_UTCID04()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roomId = Guid.NewGuid();

        _userContextMock.Setup(x => x.UserId).Returns(userId);
        _userRepoMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(new User { UserId = userId, FullName = "New Player" });

        var participant = new RoomParticipant
        {
            UserId = userId,
            RoomId = roomId,
            TeamAssignment = TeamAssignment.Unassigned, // Started as Unassigned
            IsCaptain = false                           // Not captain initially
        };

        var room = new MatchRoom
        {
            RoomId = roomId,
            Status = RoomStatus.Open,
            RoomParticipants = new List<RoomParticipant> { participant }
        };

        _matchRoomRepoMock.Setup(x => x.GetRoomWithParticipantsForUpdateAsync(roomId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(room);

        // Mock: new team currently has NO captain
        _participantRepoMock.Setup(x => x.HasTeamCaptainAsync(roomId, TeamAssignment.B, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var command = new UpdateParticipantCommand(roomId, "B", null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        participant.TeamAssignment.Should().Be(TeamAssignment.B);
        participant.IsCaptain.Should().BeTrue(); // Promoted to captain since it was empty

        _participantRepoMock.Verify(x => x.Update(participant), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // Covers UTCID05 from CSV
    [Fact]
    public async Task Handle_JustChangingPosition_ParticipantPositionUpdated_UTCID05()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roomId = Guid.NewGuid();

        _userContextMock.Setup(x => x.UserId).Returns(userId);
        _userRepoMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(new User { UserId = userId, FullName = "Fixed Player" });

        var participant = new RoomParticipant
        {
            UserId = userId,
            RoomId = roomId,
            TeamAssignment = TeamAssignment.A, // Same team
            Position = "Defender",                 // Old position
            IsCaptain = false
        };

        var room = new MatchRoom
        {
            RoomId = roomId,
            Status = RoomStatus.Open,
            RoomParticipants = new List<RoomParticipant> { participant }
        };

        _matchRoomRepoMock.Setup(x => x.GetRoomWithParticipantsForUpdateAsync(roomId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(room);

        // Act: Command changes only position, same team string
        var command = new UpdateParticipantCommand(roomId, "A", "Striker");
        
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        participant.TeamAssignment.Should().Be(TeamAssignment.A); // unchanged
        participant.Position.Should().Be("Striker"); // updated

        // Captain succession should NOT trigger
        _participantRepoMock.Verify(x => x.AssignNewCaptainAsync(It.IsAny<Guid>(), It.IsAny<TeamAssignment>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);

        _participantRepoMock.Verify(x => x.Update(participant), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        
        // Ensure SignalR notification was sent
        _hubServiceMock.Verify(x => x.NotifyParticipantUpdatedAsync(roomId, userId, It.IsAny<string>(), It.IsAny<string>(), "A", "Striker", It.IsAny<CancellationToken>()), Times.Once);
    }
}