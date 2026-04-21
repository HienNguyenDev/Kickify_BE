namespace Kickify.Application.Features.Achievements.Commands.ClaimMyAchievement;

public record ClaimMyAchievementResponse(
    Guid AchievementId,
    string AchievementName,
    DateTime EarnedAt,
    string Status
);
