using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Jobs;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace Kickify.Application.Features.MatchRooms.Commands.VoteMatchResult;

public class VoteMatchResultCommandHandler : ICommandHandler<VoteMatchResultCommand, VoteMatchResultResponse>
{
    private readonly IMatchRoomRepository _matchRoomRepository;
    private readonly IRoomParticipantRepository _roomParticipantRepository;
    private readonly IMatchResultVoteRepository _matchResultVoteRepository;
    private readonly IMatchLifecycleService _matchLifecycleService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;
    private readonly ILogger<VoteMatchResultCommandHandler> _logger;

    private const double VoteThresholdPercentage = 0.6; // 60%

    public VoteMatchResultCommandHandler(
        IMatchRoomRepository matchRoomRepository,
        IRoomParticipantRepository roomParticipantRepository,
        IMatchResultVoteRepository matchResultVoteRepository,
        IMatchLifecycleService matchLifecycleService,
        IUnitOfWork unitOfWork,
        IUserContext userContext,
        ILogger<VoteMatchResultCommandHandler> logger)
    {
        _matchRoomRepository = matchRoomRepository;
        _roomParticipantRepository = roomParticipantRepository;
        _matchResultVoteRepository = matchResultVoteRepository;
        _matchLifecycleService = matchLifecycleService;
        _unitOfWork = unitOfWork;
        _userContext = userContext;
        _logger = logger;
    }

    public async Task<Result<VoteMatchResultResponse>> Handle(VoteMatchResultCommand request, CancellationToken cancellationToken)
    {
        var userId = _userContext.UserId;

        // Get room
        var room = await _matchRoomRepository.GetByIdAsync(request.RoomId);
        if (room == null)
        {
            return Result.Failure<VoteMatchResultResponse>(MatchRoomErrors.NotFound(request.RoomId));
        }

        // Check room status - only allow voting in Reviewing phase
        if (room.Status != RoomStatus.Reviewing)
        {
            return Result.Failure<VoteMatchResultResponse>(MatchRoomErrors.VoteNotAllowed);
        }

        // Check if user is a participant
        var isParticipant = await _roomParticipantRepository.IsUserInRoomAsync(request.RoomId, userId, cancellationToken);
        if (!isParticipant)
        {
            return Result.Failure<VoteMatchResultResponse>(MatchRoomErrors.NotParticipant);
        }

        // Check if already voted
        var hasVoted = await _matchResultVoteRepository.HasUserVotedAsync(request.RoomId, userId, cancellationToken);
        if (hasVoted)
        {
            return Result.Failure<VoteMatchResultResponse>(MatchRoomErrors.AlreadyVoted);
        }

        // Create vote
        var vote = new MatchResultVote
        {
            VoteId = Guid.NewGuid(),
            RoomId = request.RoomId,
            UserId = userId,
            Vote = request.Vote,
            VotedAt = DateTime.UtcNow
        };

        await _matchResultVoteRepository.AddAsync(vote);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Get vote count and check threshold
        var voteCount = await _matchResultVoteRepository.GetVoteCountByRoomAsync(request.RoomId, cancellationToken);
        var totalParticipants = room.FilledSlots;
        var votePercentage = (double)voteCount / totalParticipants;
        var thresholdReached = votePercentage >= VoteThresholdPercentage;

        _logger.LogInformation("User {UserId} voted {Vote} for room {RoomId}. Votes: {VoteCount}/{Total} ({Percentage:P0})",
            userId, request.Vote, request.RoomId, voteCount, totalParticipants, votePercentage);

        // If threshold reached, finalize early
        if (thresholdReached)
        {
            _logger.LogInformation("Vote threshold reached for room {RoomId}. Triggering early finalization.", request.RoomId);
            await _matchLifecycleService.CheckAndFinalizeIfThresholdMetAsync(request.RoomId);
        }

        var message = thresholdReached
            ? "Vote ghi nh?n thành công. ?ã ?? 60% ng??i ch?i vote, k?t qu? s? ???c ch?t ngay."
            : $"Vote ghi nh?n thành công. Hi?n t?i {voteCount}/{totalParticipants} ng??i ?ã vote.";

        return Result.Success(new VoteMatchResultResponse(
            request.RoomId,
            userId,
            request.Vote,
            vote.VotedAt,
            voteCount,
            totalParticipants,
            votePercentage,
            thresholdReached,
            message
        ));
    }
}
