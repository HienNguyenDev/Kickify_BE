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

    public CreateMatchFeedbackCommandHandler(
        IMatchFeedbackRepository matchFeedbackRepository,
        IMatchRoomRepository matchRoomRepository,
        IRoomParticipantRepository roomParticipantRepository,
        IUnitOfWork unitOfWork)
    {
        _matchFeedbackRepository = matchFeedbackRepository;
        _matchRoomRepository = matchRoomRepository;
        _roomParticipantRepository = roomParticipantRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CreateMatchFeedbackCommandResponse>> Handle(CreateMatchFeedbackCommand request, CancellationToken cancellationToken)
    {
        var matchRoom = await _matchRoomRepository.GetByIdAsync(request.MatchId);
        if (matchRoom is null)
            return Result.Failure<CreateMatchFeedbackCommandResponse>(MatchRoomErrors.NotFound(request.MatchId));

        if (matchRoom.Status != RoomStatus.Reviewing)
            return Result.Failure<CreateMatchFeedbackCommandResponse>(MatchFeedbackErrors.MatchNotReviewing);

        var isRevieweeInMatch = await _roomParticipantRepository.IsUserInRoomAsync(request.MatchId, request.RevieweeId, cancellationToken);
        if (!isRevieweeInMatch)
            return Result.Failure<CreateMatchFeedbackCommandResponse>(MatchFeedbackErrors.RevieweeNotInMatch);

        var createdFeedbacks = new List<MatchFeedback>();

        foreach (var item in request.Feedbacks)
        {
            if (item.ReviewerId == request.RevieweeId)
                return Result.Failure<CreateMatchFeedbackCommandResponse>(MatchFeedbackErrors.CannotReviewYourself);

            var isReviewerInMatch = await _roomParticipantRepository.IsUserInRoomAsync(request.MatchId, item.ReviewerId, cancellationToken);
            if (!isReviewerInMatch)
                return Result.Failure<CreateMatchFeedbackCommandResponse>(MatchFeedbackErrors.ReviewerNotInMatch);

            var alreadyReviewed = await _matchFeedbackRepository.HasUserReviewedAsync(request.MatchId, item.ReviewerId, request.RevieweeId, cancellationToken);
            if (alreadyReviewed)
                return Result.Failure<CreateMatchFeedbackCommandResponse>(MatchFeedbackErrors.AlreadyReviewed);

            var feedback = new MatchFeedback
            {
                FeedbackId = item.FeedbackId ?? Guid.NewGuid(),
                MatchId = request.MatchId,
                ReviewerId = item.ReviewerId,
                RevieweeId = request.RevieweeId,
                Rating = item.Rating,
                Comment = item.Comment ?? string.Empty,
                CreatedAt = DateTime.UtcNow
            };

            await _matchFeedbackRepository.AddAsync(feedback);
            createdFeedbacks.Add(feedback);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreateMatchFeedbackCommandResponse
        {
            MatchId = request.MatchId,
            RevieweeId = request.RevieweeId,
            Feedbacks = createdFeedbacks.Select(f => new FeedbackResultItemDto
            {
                FeedbackId = f.FeedbackId,
                ReviewerId = f.ReviewerId,
                Rating = f.Rating,
                Comment = f.Comment,
                CreatedAt = f.CreatedAt
            }).ToList()
        });
    }
}
