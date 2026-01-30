using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.MatchFeedbacks.Commands.CreateMatchFeedback;

public class CreateMatchFeedbackCommandHandler : ICommandHandler<CreateMatchFeedbackCommand, CreateMatchFeedbackCommandResponse>
{
    private readonly IMatchFeedbackRepository _matchFeedbackRepository;
    private readonly IMatchRoomRepository _matchRoomRepository;
    private readonly IRoomParticipantRepository _roomParticipantRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;

    public CreateMatchFeedbackCommandHandler(
        IMatchFeedbackRepository matchFeedbackRepository,
        IMatchRoomRepository matchRoomRepository,
        IRoomParticipantRepository roomParticipantRepository,
        IUnitOfWork unitOfWork,
        IUserContext userContext)
    {
        _matchFeedbackRepository = matchFeedbackRepository;
        _matchRoomRepository = matchRoomRepository;
        _roomParticipantRepository = roomParticipantRepository;
        _unitOfWork = unitOfWork;
        _userContext = userContext;
    }

    public async Task<Result<CreateMatchFeedbackCommandResponse>> Handle(CreateMatchFeedbackCommand request, CancellationToken cancellationToken)
    {
        var reviewerId = _userContext.UserId;

        // Check if reviewer is trying to review themselves
        if (reviewerId == request.RevieweeId)
        {
            return Result.Failure<CreateMatchFeedbackCommandResponse>(MatchFeedbackErrors.CannotReviewYourself);
        }

        // Get match room
        var matchRoom = await _matchRoomRepository.GetByIdAsync(request.MatchId);
        if (matchRoom is null)
        {
            return Result.Failure<CreateMatchFeedbackCommandResponse>(MatchRoomErrors.NotFound(request.MatchId));
        }

        // Check if match is completed
        if (matchRoom.Status != RoomStatus.Completed)
        {
            return Result.Failure<CreateMatchFeedbackCommandResponse>(MatchFeedbackErrors.MatchNotCompleted);
        }

        // Check if reviewer was a participant
        var isReviewerInMatch = await _roomParticipantRepository.IsUserInRoomAsync(request.MatchId, reviewerId, cancellationToken);
        if (!isReviewerInMatch)
        {
            return Result.Failure<CreateMatchFeedbackCommandResponse>(MatchFeedbackErrors.ReviewerNotInMatch);
        }

        // Check if reviewee was a participant
        var isRevieweeInMatch = await _roomParticipantRepository.IsUserInRoomAsync(request.MatchId, request.RevieweeId, cancellationToken);
        if (!isRevieweeInMatch)
        {
            return Result.Failure<CreateMatchFeedbackCommandResponse>(MatchFeedbackErrors.RevieweeNotInMatch);
        }

        // Check if already reviewed
        var existingFeedback = await _matchFeedbackRepository.HasUserReviewedAsync(request.MatchId, reviewerId, request.RevieweeId, cancellationToken);
        if (existingFeedback)
        {
            return Result.Failure<CreateMatchFeedbackCommandResponse>(MatchFeedbackErrors.AlreadyReviewed);
        }

        // Create feedback
        var feedback = new MatchFeedback
        {
            FeedbackId = Guid.NewGuid(),
            MatchId = request.MatchId,
            ReviewerId = reviewerId,
            RevieweeId = request.RevieweeId,
            Rating = request.Rating,
            Comment = request.Comment ?? string.Empty,
            CreatedAt = DateTime.UtcNow
        };

        await _matchFeedbackRepository.AddAsync(feedback);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new CreateMatchFeedbackCommandResponse
        {
            FeedbackId = feedback.FeedbackId,
            MatchId = feedback.MatchId,
            ReviewerId = feedback.ReviewerId,
            RevieweeId = feedback.RevieweeId,
            Rating = feedback.Rating,
            Comment = feedback.Comment,
            CreatedAt = feedback.CreatedAt
        };

        return Result.Success(response);
    }
}
