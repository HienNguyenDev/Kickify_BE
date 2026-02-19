namespace Kickify.Application.Features.Achievements.Commands.DeleteAchievement;

public record DeleteAchievementResponse(Guid AchievementId, bool Deleted);
