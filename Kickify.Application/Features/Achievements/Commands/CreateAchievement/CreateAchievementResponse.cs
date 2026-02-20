namespace Kickify.Application.Features.Achievements.Commands.CreateAchievement;

public record CreateAchievementResponse(
    Guid AchievementId,
    string Name,
    string? Description,
    string? BadgeIconUrl,
    string CriteriaType,
    int CriteriaValue,
    DateTime CreatedAt
);
