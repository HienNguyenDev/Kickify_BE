namespace Kickify.Application.Features.Achievements.Queries.GetAllAchievements;

public record GetAllAchievementsResponse(
    List<AchievementItemDto> Achievements,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);

public record AchievementItemDto(
    Guid AchievementId,
    string Name,
    string? Description,
    string? BadgeIconUrl,
    string CriteriaType,
    int CriteriaValue,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
