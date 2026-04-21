namespace Kickify.Application.Features.Achievements.Queries.GetMyAchievements;

public record GetMyAchievementsResponse(
    List<MyAchievementItemDto> Achievements,
    int TotalAchievements,
    int UnlockedCount
);

public record MyAchievementItemDto(
    Guid AchievementId,
    string Name,
    string? Description,
    string? BadgeIconUrl,
    string CriteriaType,
    int TargetValue,
    int CurrentProgress,
    decimal ProgressPercent,
    string Status,
    bool IsUnlocked,
    DateTime? EarnedAt
);
