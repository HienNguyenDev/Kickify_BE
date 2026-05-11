using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Abstractions.Services;
using Kickify.Domain.Common;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Kickify.Application.Features.PlayerProfiles.Queries.GetLeaderboard;

public class GetLeaderboardQueryHandler : IQueryHandler<GetLeaderboardQuery, GetLeaderboardResponse>
{
    private readonly IPlayerProfileRepository _playerProfileRepository;
    private readonly ILeaderboardCacheService _leaderboardCacheService;
    private readonly ILogger<GetLeaderboardQueryHandler> _logger;

    public GetLeaderboardQueryHandler(
        IPlayerProfileRepository playerProfileRepository,
        ILeaderboardCacheService leaderboardCacheService,
        ILogger<GetLeaderboardQueryHandler> logger)
    {
        _playerProfileRepository = playerProfileRepository;
        _leaderboardCacheService = leaderboardCacheService;
        _logger = logger;
    }

    public async Task<Result<GetLeaderboardResponse>> Handle(GetLeaderboardQuery request, CancellationToken cancellationToken)
    {
        // Th? l?y t? cache tr??c
        var cachedLeaderboard = await _leaderboardCacheService.GetLeaderboardCacheAsync(cancellationToken);

        if (!string.IsNullOrEmpty(cachedLeaderboard))
        {
            try
            {
                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var response = JsonSerializer.Deserialize<GetLeaderboardResponse>(cachedLeaderboard, jsonOptions);
                
                if (response != null)
                {
                    _logger.LogInformation("Returning leaderboard from cache with {Count} players", response.Leaderboard.Count);
                    return Result.Success(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deserializing cached leaderboard, falling back to database");
            }
        }

        // N?u cache không có ho?c l?i, query t? database
        _logger.LogWarning("Cache miss or error, querying leaderboard from database");
        
        var topPlayersWithChange = await _playerProfileRepository.GetTopPlayersByEloWithChangeAsync(50, cancellationToken);

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

        var fallbackResponse = new GetLeaderboardResponse(leaderboard);

        // Cache l?i k?t qu? cho l?n sau
        try
        {
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };

            var leaderboardJson = JsonSerializer.Serialize(fallbackResponse, jsonOptions);
            await _leaderboardCacheService.SetLeaderboardCacheAsync(leaderboardJson, cancellationToken);
            
            _logger.LogInformation("Cached fresh leaderboard data with {Count} players", leaderboard.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error caching leaderboard data");
        }

        return Result.Success(fallbackResponse);
    }
}
