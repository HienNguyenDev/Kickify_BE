using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Abstractions.Services;
using Kickify.Domain.Common;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kickify.Application.Features.MatchRooms.Commands.SubmitAfkVote;

public class SubmitAfkVoteCommandHandler : ICommandHandler<SubmitAfkVoteCommand, SubmitAfkVoteResponse>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserContext _userContext;
    private readonly IAfkVoteService _afkVoteService;
    private readonly IUnitOfWork _unitOfWork;

    public SubmitAfkVoteCommandHandler(
        IApplicationDbContext dbContext,
        IUserContext userContext,
        IAfkVoteService afkVoteService,
        IUnitOfWork unitOfWork)
    {
        _dbContext = dbContext;
        _userContext = userContext;
        _afkVoteService = afkVoteService;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Submit AFK votes for teammates in a match room during reviewing phase.
    /// </summary>
    public async Task<Result<SubmitAfkVoteResponse>> Handle(SubmitAfkVoteCommand request, CancellationToken cancellationToken)
    {
        var voterId = _userContext.UserId;
        var room = await _dbContext.MatchRooms.FirstOrDefaultAsync(x => x.RoomId == request.MatchRoomId, cancellationToken);
        if (room is null)
        {
            return Result.Failure<SubmitAfkVoteResponse>(MatchRoomErrors.NotFound(request.MatchRoomId));
        }

        if (room.Status != RoomStatus.Reviewing)
        {
            return Result.Failure<SubmitAfkVoteResponse>(MatchRoomErrors.NotInReviewingPhase);
        }

        var voterParticipant = await _dbContext.RoomParticipants
            .FirstOrDefaultAsync(x => x.RoomId == request.MatchRoomId && x.UserId == voterId, cancellationToken);

        if (voterParticipant is null)
        {
            return Result.Failure<SubmitAfkVoteResponse>(MatchRoomErrors.NotParticipant);
        }

        var hasSubmitted = await _dbContext.AfkVotes
            .AnyAsync(x => x.MatchRoomId == request.MatchRoomId && x.VoterId == voterId, cancellationToken);

        if (hasSubmitted)
        {
            return Result.Failure<SubmitAfkVoteResponse>(MatchRoomErrors.AlreadySubmittedAfkVote);
        }

        var targetIds = request.TargetPlayerIds.Distinct().ToList();
        if (targetIds.Contains(voterId))
        {
            return Result.Failure<SubmitAfkVoteResponse>(MatchRoomErrors.CannotVoteSelfAfk);
        }

        var targetParticipants = await _dbContext.RoomParticipants
            .Where(x => x.RoomId == request.MatchRoomId && targetIds.Contains(x.UserId))
            .ToListAsync(cancellationToken);

        if (targetParticipants.Count != targetIds.Count)
        {
            return Result.Failure<SubmitAfkVoteResponse>(MatchRoomErrors.NotParticipant);
        }

        var hasOpposingTeamTarget = targetParticipants.Any(x => x.TeamAssignment != voterParticipant.TeamAssignment);
        if (hasOpposingTeamTarget)
        {
            return Result.Failure<SubmitAfkVoteResponse>(MatchRoomErrors.CannotVoteOpposingTeamAfk);
        }

        var now = DateTime.UtcNow;
        var votes = targetIds.Select(targetId => new AfkVote
        {
            Id = Guid.NewGuid(),
            MatchRoomId = request.MatchRoomId,
            VoterId = voterId,
            TargetPlayerId = targetId,
            Team = voterParticipant.TeamAssignment,
            CreatedAt = now
        }).ToList();

        _dbContext.AfkVotes.AddRange(votes);

        await _afkVoteService.RecalculateMatchAfkStatusAsync(request.MatchRoomId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new SubmitAfkVoteResponse(
            request.MatchRoomId,
            voterId,
            votes.Count,
            "AFK votes submitted successfully."));
    }
}
