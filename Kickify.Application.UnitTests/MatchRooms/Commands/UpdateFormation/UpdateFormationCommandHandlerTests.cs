using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Abstractions.Services;
using Kickify.Application.Features.MatchRooms.Commands.UpdateFormation;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Kickify.Application.UnitTests.MatchRooms.Commands.UpdateFormation;

public class UpdateFormationCommandHandlerTests
{
    private readonly Mock<IMatchRoomRepository> _matchRoomRepoMock = new();
    private readonly Mock<IRoomParticipantRepository> _participantRepoMock = new();
    private readonly Mock<IMatchFormationRepository> _formationRepoMock = new();
    private readonly Mock<IFormationAssignmentRepository> _assignmentRepoMock = new();
    private readonly Mock<IMatchRoomHubService> _hubServiceMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IUserContext> _userContextMock = new();
    private readonly Mock<ILogger<UpdateFormationCommandHandler>> _loggerMock = new();

    private readonly UpdateFormationCommandHandler _sut;

    public UpdateFormationCommandHandlerTests()
    {
        _sut = new UpdateFormationCommandHandler(
            _matchRoomRepoMock.Object, _participantRepoMock.Object, _formationRepoMock.Object,
            _assignmentRepoMock.Object, _hubServiceMock.Object, _unitOfWorkMock.Object,
            _userContextMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_RoomNotOpen_ReturnsRoomNotActiveError()
    {
        // Covers UTCID27 from CSV
        // Arrange
        var userId = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        
        _userContextMock.Setup(u => u.UserId).Returns(userId);
        
        var room = new MatchRoom 
        { 
            RoomId = roomId, 
            Status = RoomStatus.Locked // Status is not Open
        };
        
        _matchRoomRepoMock.Setup(repo => repo.GetRoomWithParticipantsAsync(roomId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(room);

        var command = new UpdateFormationCommand(roomId, "A", "4-3-3", new List<FormationSlotAssignment>());

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be(MatchRoomErrors.RoomNotActive.Code);
    }

    [Fact]
    public async Task Handle_NotCaptainOrUnassigned_ReturnsNotCaptainError()
    {
        // Covers UTCID28 from CSV
        // Arrange
        var userId = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        
        _userContextMock.Setup(u => u.UserId).Returns(userId);
        
        var room = new MatchRoom 
        { 
            RoomId = roomId, 
            Status = RoomStatus.Open,
            RoomParticipants = new List<RoomParticipant>
            {
                new RoomParticipant { UserId = userId, TeamAssignment = TeamAssignment.A, IsCaptain = false } // User is not Captain
            }
        };
        
        _matchRoomRepoMock.Setup(repo => repo.GetRoomWithParticipantsAsync(roomId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(room);

        var command = new UpdateFormationCommand(roomId, "A", "4-3-3", new List<FormationSlotAssignment>());

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be(MatchRoomErrors.NotCaptain.Code);
    }

    [Fact]
    public async Task Handle_InvalidFormationName_ReturnsInvalidFormationError()
    {
        // Covers UTCID29 from CSV
        // Arrange
        var userId = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        var format = 5; // Assuming 5v5
        
        _userContextMock.Setup(u => u.UserId).Returns(userId);
        
        var room = new MatchRoom 
        { 
            RoomId = roomId, 
            Status = RoomStatus.Open,
            MatchFormat = MatchFormat.FiveVsFive,
            RoomParticipants = new List<RoomParticipant>
            {
                new RoomParticipant { UserId = userId, TeamAssignment = TeamAssignment.A, IsCaptain = true }
            }
        };
        
        _matchRoomRepoMock.Setup(repo => repo.GetRoomWithParticipantsAsync(roomId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(room);

        var command = new UpdateFormationCommand(roomId, "A", "UNKNOWN-FORMATION", new List<FormationSlotAssignment>());

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be(MatchRoomErrors.InvalidFormation("UNKNOWN-FORMATION", format.ToString()).Code);
    }

    [Fact]
    public async Task Handle_DuplicateSlotAssignment_ReturnsDuplicateSlotAssignmentError()
    {
        // Covers UTCID30 from CSV
        // Arrange
        var userId = Guid.NewGuid();
        var player1Id = Guid.NewGuid();
        var player2Id = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        
        _userContextMock.Setup(u => u.UserId).Returns(userId);
        
        var room = new MatchRoom 
        { 
            RoomId = roomId, 
            Status = RoomStatus.Open,
            MatchFormat = MatchFormat.FiveVsFive,
            RoomParticipants = new List<RoomParticipant>
            {
                new RoomParticipant { UserId = userId, TeamAssignment = TeamAssignment.A, IsCaptain = true },
                new RoomParticipant { UserId = player1Id, TeamAssignment = TeamAssignment.A },
                new RoomParticipant { UserId = player2Id, TeamAssignment = TeamAssignment.A }
            }
        };
        
        _matchRoomRepoMock.Setup(repo => repo.GetRoomWithParticipantsAsync(roomId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(room);

        var command = new UpdateFormationCommand(roomId, "A", "2-2", new List<FormationSlotAssignment>
        {
            new FormationSlotAssignment(player1Id, "DF-1"),
            new FormationSlotAssignment(player2Id, "DF-1") // Duplicate slot
        });

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be(MatchRoomErrors.DuplicateSlotAssignment("DF-1").Code);
    }

    [Fact]
    public async Task Handle_ValidAssignments_UpdatesFormationAndReturnsSuccess()
    {
        // Covers UTCID31 from CSV
        // Arrange
        var userId = Guid.NewGuid();
        var player1Id = Guid.NewGuid();
        var player2Id = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        
        _userContextMock.Setup(u => u.UserId).Returns(userId);
        
        var room = new MatchRoom 
        { 
            RoomId = roomId, 
            Status = RoomStatus.Open,
            MatchFormat = MatchFormat.FiveVsFive,
            RoomParticipants = new List<RoomParticipant>
            {
                new RoomParticipant { UserId = userId, TeamAssignment = TeamAssignment.A, IsCaptain = true },
                new RoomParticipant { UserId = player1Id, TeamAssignment = TeamAssignment.A },
                new RoomParticipant { UserId = player2Id, TeamAssignment = TeamAssignment.A }
            }
        };
        
        _matchRoomRepoMock.Setup(repo => repo.GetRoomWithParticipantsAsync(roomId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(room);

        _formationRepoMock.Setup(repo => repo.GetFormationByRoomAndTeamAsync(roomId, TeamAssignment.A, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MatchFormation?)null);

        // using valid partial assignments
        var command = new UpdateFormationCommand(roomId, "A", "1-2-1", new List<FormationSlotAssignment>
        {
            new FormationSlotAssignment(player1Id, "DF-0")
        });

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue($"Error was: {result.Error?.Code}");
        
        _formationRepoMock.Verify(repo => repo.AddAsync(It.IsAny<MatchFormation>()), Times.Once);
        _assignmentRepoMock.Verify(repo => repo.AddAsync(It.IsAny<FormationAssignment>()), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _hubServiceMock.Verify(hub => hub.NotifyFormationUpdatedAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
