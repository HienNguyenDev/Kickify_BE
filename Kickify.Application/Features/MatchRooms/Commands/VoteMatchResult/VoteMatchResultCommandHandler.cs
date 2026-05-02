using Kickify.Application.Abstractions.Authentication;
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
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;
    private readonly ILogger<VoteMatchResultCommandHandler> _logger;

    public VoteMatchResultCommandHandler(
        IMatchRoomRepository matchRoomRepository,
        IRoomParticipantRepository roomParticipantRepository,
        IMatchResultVoteRepository matchResultVoteRepository,
        IUnitOfWork unitOfWork,
        IUserContext userContext,
        ILogger<VoteMatchResultCommandHandler> logger)
    {
        _matchRoomRepository = matchRoomRepository;
        _roomParticipantRepository = roomParticipantRepository;
        _matchResultVoteRepository = matchResultVoteRepository;
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
            return Result.Failure<VoteMatchResultResponse>(MatchRoomErrors.VotingPeriodClosed);
        }

        // Check if user is a participant
        var isParticipant = await _roomParticipantRepository.IsUserInRoomAsync(request.RoomId, userId, cancellationToken);
        if (!isParticipant)
        {
            return Result.Failure<VoteMatchResultResponse>(MatchRoomErrors.NotParticipant);
        }

        if (userId != room.HostId)
        {
            return Result.Failure<VoteMatchResultResponse>(MatchRoomErrors.OnlyHostCanSubmitMatchResult);
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

        // Get current vote count
        var voteCount = await _matchResultVoteRepository.GetVoteCountByRoomAsync(request.RoomId, cancellationToken);
        var totalParticipants = room.FilledSlots;

        _logger.LogInformation("Host {UserId} submitted match result {Vote} for room {RoomId}. Reviewing continues until the scheduled 22h window ends.",
            userId, request.Vote, request.RoomId);

        var message = "Đã ghi nhận kết quả từ host. Phòng vẫn ở Reviewing cho đến khi hết cửa sổ 22 giờ sau khi trận kết thúc, rồi mới chuyển Completed.";

        return Result.Success(new VoteMatchResultResponse(
            request.RoomId,
            userId,
            request.Vote,
            vote.VotedAt,
            voteCount,
            totalParticipants,
            message
        ));
    }
}
