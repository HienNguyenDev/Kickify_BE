using FluentAssertions;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Features.MatchFeedbacks.Commands.CreateMatchFeedback;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;
using Moq;

namespace Kickify.Application.UnitTests.MatchFeedbacks.Commands.CreateMatchFeedback;

public class CreateMatchFeedbackCommandHandlerTests
{
    private readonly Mock<IMatchFeedbackRepository> _matchFeedbackRepositoryMock;
    private readonly Mock<IMatchRoomRepository> _matchRoomRepositoryMock;
    private readonly Mock<IRoomParticipantRepository> _roomParticipantRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateMatchFeedbackCommandHandler _handler;

    public CreateMatchFeedbackCommandHandlerTests()
    {
        _matchFeedbackRepositoryMock = new Mock<IMatchFeedbackRepository>();
        _matchRoomRepositoryMock = new Mock<IMatchRoomRepository>();
        _roomParticipantRepositoryMock = new Mock<IRoomParticipantRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new CreateMatchFeedbackCommandHandler(
            _matchFeedbackRepositoryMock.Object,
            _matchRoomRepositoryMock.Object,
            _roomParticipantRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    // Covers UTCID40
    [Fact]
    public async Task Handle_CreateFeedback_WhenMatchNotReviewing_ShouldReturnError_UTCID40()
    {
        // Arrange
        var command = new CreateMatchFeedbackCommand { MatchId = Guid.NewGuid() };
        var matchRoom = new MatchRoom { RoomId = command.MatchId, Status = RoomStatus.Locked }; // Not Reviewing

        _matchRoomRepositoryMock.Setup(repo => repo.GetByIdAsync(command.MatchId))
            .ReturnsAsync(matchRoom);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(MatchFeedbackErrors.MatchNotReviewing);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // Covers UTCID41
    [Fact]
    public async Task Handle_CreateFeedback_WhenReviewerNotInMatch_ShouldReturnError_UTCID41()
    {
        // Arrange
        var command = new CreateMatchFeedbackCommand { MatchId = Guid.NewGuid(), ReviewerId = Guid.NewGuid() };
        var matchRoom = new MatchRoom { RoomId = command.MatchId, Status = RoomStatus.Reviewing };

        _matchRoomRepositoryMock.Setup(repo => repo.GetByIdAsync(command.MatchId))
            .ReturnsAsync(matchRoom);
            
        // Reviewer is not in match
        _roomParticipantRepositoryMock.Setup(repo => repo.IsUserInRoomAsync(command.MatchId, command.ReviewerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(MatchFeedbackErrors.ReviewerNotInMatch);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // Covers UTCID42
    [Fact]
    public async Task Handle_CreateFeedback_WhenReviewingYourself_ShouldReturnError_UTCID42()
    {
        // Arrange
        var command = new CreateMatchFeedbackCommand 
        { 
            MatchId = Guid.NewGuid(), 
            ReviewerId = Guid.NewGuid(),
            Feedbacks = new List<FeedbackItemDto>()
        };
        // Add a feedback targeting the reviewer themselves
        command.Feedbacks.Add(new FeedbackItemDto { RevieweeId = command.ReviewerId, Rating = 5 });

        var matchRoom = new MatchRoom { RoomId = command.MatchId, Status = RoomStatus.Reviewing };

        _matchRoomRepositoryMock.Setup(repo => repo.GetByIdAsync(command.MatchId))
            .ReturnsAsync(matchRoom);
            
        _roomParticipantRepositoryMock.Setup(repo => repo.IsUserInRoomAsync(command.MatchId, command.ReviewerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(MatchFeedbackErrors.CannotReviewYourself);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // Covers UTCID43
    [Fact]
    public async Task Handle_CreateFeedback_WhenRevieweeNotInMatch_ShouldReturnError_UTCID43()
    {
        // Arrange
        var reviewerId = Guid.NewGuid();
        var revieweeId = Guid.NewGuid();
        var command = new CreateMatchFeedbackCommand 
        { 
            MatchId = Guid.NewGuid(), 
            ReviewerId = reviewerId,
            Feedbacks = new List<FeedbackItemDto>
            {
                new FeedbackItemDto { RevieweeId = revieweeId, Rating = 5 }
            }
        };

        var matchRoom = new MatchRoom { RoomId = command.MatchId, Status = RoomStatus.Reviewing };

        _matchRoomRepositoryMock.Setup(repo => repo.GetByIdAsync(command.MatchId))
            .ReturnsAsync(matchRoom);
            
        _roomParticipantRepositoryMock.Setup(repo => repo.IsUserInRoomAsync(command.MatchId, reviewerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Reviewee is not in match
        _roomParticipantRepositoryMock.Setup(repo => repo.IsUserInRoomAsync(command.MatchId, revieweeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(MatchFeedbackErrors.RevieweeNotInMatch);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // Covers UTCID44
    [Fact]
    public async Task Handle_CreateFeedback_WhenAlreadyReviewed_ShouldReturnError_UTCID44()
    {
        // Arrange
        var reviewerId = Guid.NewGuid();
        var revieweeId = Guid.NewGuid();
        var command = new CreateMatchFeedbackCommand 
        { 
            MatchId = Guid.NewGuid(), 
            ReviewerId = reviewerId,
            Feedbacks = new List<FeedbackItemDto>
            {
                new FeedbackItemDto { RevieweeId = revieweeId, Rating = 5 }
            }
        };

        var matchRoom = new MatchRoom { RoomId = command.MatchId, Status = RoomStatus.Reviewing };

        _matchRoomRepositoryMock.Setup(repo => repo.GetByIdAsync(command.MatchId))
            .ReturnsAsync(matchRoom);
            
        _roomParticipantRepositoryMock.Setup(repo => repo.IsUserInRoomAsync(command.MatchId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true); // Both in match

        // User already reviewed this target
        _matchFeedbackRepositoryMock.Setup(repo => repo.HasUserReviewedAsync(command.MatchId, reviewerId, revieweeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(MatchFeedbackErrors.AlreadyReviewed);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // Covers UTCID45
    [Fact]
    public async Task Handle_CreateFeedback_WhenCleanEvalBlocks_ShouldSaveSuccessfully_UTCID45()
    {
        // Arrange
        var reviewerId = Guid.NewGuid();
        var reviewee1 = Guid.NewGuid();
        var reviewee2 = Guid.NewGuid();
        var command = new CreateMatchFeedbackCommand 
        { 
            MatchId = Guid.NewGuid(), 
            ReviewerId = reviewerId,
            Feedbacks = new List<FeedbackItemDto>
            {
                new FeedbackItemDto { RevieweeId = reviewee1, Rating = 4, Comment = "Good" },
                new FeedbackItemDto { RevieweeId = reviewee2, Rating = 5, Comment = "Great" }
            }
        };

        var matchRoom = new MatchRoom { RoomId = command.MatchId, Status = RoomStatus.Reviewing };

        _matchRoomRepositoryMock.Setup(repo => repo.GetByIdAsync(command.MatchId))
            .ReturnsAsync(matchRoom);
            
        _roomParticipantRepositoryMock.Setup(repo => repo.IsUserInRoomAsync(command.MatchId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true); // Everyone in match

        _matchFeedbackRepositoryMock.Setup(repo => repo.HasUserReviewedAsync(command.MatchId, reviewerId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false); // No prior reviews

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        // 2 blocks provided => 2 distinct calls to add feedback
        _matchFeedbackRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<MatchFeedback>()), Times.Exactly(2));
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        
        result.Value.Feedbacks.Should().HaveCount(2);
        result.Value.ReviewerId.Should().Be(reviewerId);
        result.Value.MatchId.Should().Be(command.MatchId);
    }
}
