using Kickify.Application.Abstractions.Services;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;

namespace Kickify.Infrastructure.Services;

public class LeaderboardCacheService : ILeaderboardCacheService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<LeaderboardCacheService> _logger;
    private const string LeaderboardCacheKey = "leaderboard:top50";
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromHours(24);

    public LeaderboardCacheService(
        IConnectionMultiplexer redis,
        ILogger<LeaderboardCacheService> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task<string?> GetLeaderboardCacheAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            var cached = await db.StringGetAsync(LeaderboardCacheKey);

            if (cached.HasValue)
            {
                _logger.LogInformation("Leaderboard cache hit");
                return cached.ToString();
            }

            _logger.LogInformation("Leaderboard cache miss");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting leaderboard cache from Redis");
            return null;
        }
    }

    public async Task SetLeaderboardCacheAsync(string leaderboardJson, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            await db.StringSetAsync(LeaderboardCacheKey, leaderboardJson, CacheExpiration);
            
            _logger.LogInformation("Leaderboard cache updated successfully. Expiration: {Expiration}", CacheExpiration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting leaderboard cache in Redis");
        }
    }

    public async Task ClearLeaderboardCacheAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            await db.KeyDeleteAsync(LeaderboardCacheKey);
            
            _logger.LogInformation("Leaderboard cache cleared");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing leaderboard cache from Redis");
        }
    }

    public async Task<bool> LeaderboardCacheExistsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            return await db.KeyExistsAsync(LeaderboardCacheKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking leaderboard cache existence in Redis");
            return false;
        }
    }
}
