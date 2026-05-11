namespace Kickify.Application.Abstractions.Services;

public interface ILeaderboardCacheService
{
    /// <summary>
    /// Get cached leaderboard data
    /// </summary>
    Task<string?> GetLeaderboardCacheAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Set leaderboard cache with 24-hour expiration
    /// </summary>
    Task SetLeaderboardCacheAsync(string leaderboardJson, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Clear leaderboard cache
    /// </summary>
    Task ClearLeaderboardCacheAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Check if leaderboard cache exists
    /// </summary>
    Task<bool> LeaderboardCacheExistsAsync(CancellationToken cancellationToken = default);
}
