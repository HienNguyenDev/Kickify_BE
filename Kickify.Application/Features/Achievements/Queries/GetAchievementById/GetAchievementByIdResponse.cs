namespace Kickify.Application.Features.Achievements.Queries.GetAchievementById;

public record GetAchievementByIdResponse(
    Guid AchievementId,
    string Name,
    string? Description,
    string? BadgeIconUrl,
    string CriteriaType,
    int CriteriaValue,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
