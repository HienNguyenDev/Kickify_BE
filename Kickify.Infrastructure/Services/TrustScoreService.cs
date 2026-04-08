using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Services;
using Kickify.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Kickify.Infrastructure.Services;

public class TrustScoreService : ITrustScoreService
{
    private readonly IApplicationDbContext _dbContext;

    public TrustScoreService(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Recalculate trust score for a player based on AFK, cancellation and participation signals.
    /// </summary>
    public async Task RecalculateAsync(Guid playerId, CancellationToken cancellationToken = default)
    {
        var profile = await _dbContext.PlayerProfiles
            .FirstOrDefaultAsync(x => x.UserId == playerId, cancellationToken);

        if (profile is null)
        {
            return;
        }

        var joinedMatches = _dbContext.RoomParticipants.Where(x => x.UserId == playerId);
        var totalJoined = await joinedMatches.CountAsync(cancellationToken);

        var cancelledMatches = await joinedMatches
            .Where(x => x.MatchRoom.Status == RoomStatus.Cancelled)
            .CountAsync(cancellationToken);

        var completedMatchIds = await joinedMatches
            .Where(x => x.MatchRoom.Status == RoomStatus.Completed)
            .Select(x => x.RoomId)
            .ToListAsync(cancellationToken);

        var completedMatches = completedMatchIds.Count;

        var submittedVoteRoomIds = await _dbContext.MatchResultVotes
            .Where(x => x.UserId == playerId && completedMatchIds.Contains(x.RoomId))
            .Select(x => x.RoomId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var submittedRatingRoomIds = await _dbContext.MatchFeedbacks
            .Where(x => x.ReviewerId == playerId && completedMatchIds.Contains(x.MatchId))
            .Select(x => x.MatchId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var interactionRoomIds = submittedVoteRoomIds.Union(submittedRatingRoomIds).ToHashSet();
        var noShowCount = completedMatchIds.Count(x => !interactionRoomIds.Contains(x));

        var cancelRate = totalJoined == 0 ? 0d : (double)cancelledMatches / totalJoined;
        var completedBonus = Math.Min(completedMatches * 0.5d, 20d);

        var score = 100d
                    - (cancelRate * 20d)
                    - (noShowCount * 8d)
                    + completedBonus;

        profile.TrustScore = (int)Math.Clamp(Math.Round(score, MidpointRounding.AwayFromZero), 0, 100);
    }
}
