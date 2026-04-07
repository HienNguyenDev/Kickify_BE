using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Services;
using Microsoft.EntityFrameworkCore;

namespace Kickify.Infrastructure.Services;

public class AfkVoteService : IAfkVoteService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITrustScoreService _trustScoreService;

    public AfkVoteService(IApplicationDbContext dbContext, ITrustScoreService trustScoreService)
    {
        _dbContext = dbContext;
        _trustScoreService = trustScoreService;
    }

    /// <summary>
    /// Recalculate AFK counts and flags for all participants in a room, then refresh trust scores.
    /// </summary>
    public async Task RecalculateMatchAfkStatusAsync(Guid matchRoomId, CancellationToken cancellationToken = default)
    {
        var participants = await _dbContext.RoomParticipants
            .Where(x => x.RoomId == matchRoomId)
            .ToListAsync(cancellationToken);

        if (participants.Count == 0)
        {
            return;
        }

        var voteCounts = await _dbContext.AfkVotes
            .Where(x => x.MatchRoomId == matchRoomId)
            .GroupBy(x => x.TargetPlayerId)
            .Select(g => new { TargetPlayerId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.TargetPlayerId, x => x.Count, cancellationToken);

        var threshold = (int)Math.Ceiling(participants.Count * 0.6d);

        foreach (var participant in participants)
        {
            var votes = voteCounts.TryGetValue(participant.UserId, out var count) ? count : 0;
            participant.AfkVoteCount = votes;
            participant.IsConfirmedAfk = votes >= threshold;
        }

        foreach (var userId in participants.Select(x => x.UserId).Distinct())
        {
            await _trustScoreService.RecalculateAsync(userId, cancellationToken);
        }
    }
}
