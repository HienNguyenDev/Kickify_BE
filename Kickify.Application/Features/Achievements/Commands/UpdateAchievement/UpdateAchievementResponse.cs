namespace Kickify.Application.Features.Achievements.Commands.UpdateAchievement;

public record UpdateAchievementResponse(
    Guid AchievementId,
    string Name,
    string? Description,
    string? BadgeIconUrl,
    string CriteriaType,
    int CriteriaValue,
    DateTime UpdatedAt
);
