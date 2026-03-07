namespace Kickify.Application.Features.PlayerProfiles.Queries.GetLeaderboard;

public record GetLeaderboardResponse(
    List<LeaderboardEntry> Leaderboard
);

public record LeaderboardEntry(
    int Rank,
    Guid UserId,
    string FullName,
    string Email,
    string? AvatarUrl,
    int CurrentElo,
    int EloChange,
    int TotalMatches
);
