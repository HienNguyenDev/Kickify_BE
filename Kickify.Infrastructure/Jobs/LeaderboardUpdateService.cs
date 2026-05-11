using Hangfire;
using Kickify.Application.Abstractions.Jobs;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Abstractions.Services;
using Kickify.Application.Features.PlayerProfiles.Queries.GetLeaderboard;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Kickify.Infrastructure.Jobs;

public class LeaderboardUpdateService : ILeaderboardUpdateService
{
    private readonly IRecurringJobManager _recurringJobManager;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<LeaderboardUpdateService> _logger;

    public LeaderboardUpdateService(
        IRecurringJobManager recurringJobManager,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<LeaderboardUpdateService> logger)
    {
        _recurringJobManager = recurringJobManager;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    public void ScheduleLeaderboardUpdate()
    {
        // Ch?y m?i ngày lúc 00:00 (midnight) UTC
        _recurringJobManager.AddOrUpdate(
            "update-leaderboard-cache",
            () => UpdateLeaderboardCacheAsync(),
            Cron.Daily(0, 0), // 00:00 every day
            TimeZoneInfo.Utc);

        _logger.LogInformation("Scheduled leaderboard update job to run daily at 00:00 UTC");
    }

    public async Task UpdateLeaderboardCacheAsync()
    {
        _logger.LogInformation("Starting leaderboard cache update job");

        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var playerProfileRepository = scope.ServiceProvider.GetRequiredService<IPlayerProfileRepository>();
            var leaderboardCacheService = scope.ServiceProvider.GetRequiredService<ILeaderboardCacheService>();

            // L?y top 50 players v?i EloChange
            var topPlayersWithChange = await playerProfileRepository.GetTopPlayersByEloWithChangeAsync(50, CancellationToken.None);

            // Build leaderboard response
            var leaderboard = topPlayersWithChange.Select((item, index) => new LeaderboardEntry(
                Rank: index + 1,
                UserId: item.Profile.UserId,
                FullName: item.Profile.User.FullName ?? item.Profile.User.Email,
                Email: item.Profile.User.Email,
                AvatarUrl: item.Profile.User.AvatarUrl,
                CurrentElo: item.Profile.CurrentElo,
                CurrentRank: item.Profile.CurrentRank,
                IsLegend: item.Profile.IsLegend,
                EloChange: item.LatestEloChange,
                TotalMatches: item.Profile.TotalMatches
            )).ToList();

            var response = new GetLeaderboardResponse(leaderboard);

            // Serialize và cache
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };

            var leaderboardJson = JsonSerializer.Serialize(response, jsonOptions);
            await leaderboardCacheService.SetLeaderboardCacheAsync(leaderboardJson);

            _logger.LogInformation("Leaderboard cache updated successfully with {Count} players", leaderboard.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating leaderboard cache");
        }
    }
}
