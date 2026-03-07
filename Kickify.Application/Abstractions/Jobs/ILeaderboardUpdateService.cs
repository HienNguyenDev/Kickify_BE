namespace Kickify.Application.Abstractions.Jobs;

public interface ILeaderboardUpdateService
{
    /// <summary>
    /// Update leaderboard cache - runs every 24 hours
    /// </summary>
    Task UpdateLeaderboardCacheAsync();
    
    /// <summary>
    /// Schedule recurring job to update leaderboard every 24 hours at midnight
    /// </summary>
    void ScheduleLeaderboardUpdate();
}
