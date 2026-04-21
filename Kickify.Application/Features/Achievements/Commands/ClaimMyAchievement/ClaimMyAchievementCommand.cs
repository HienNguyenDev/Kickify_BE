using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.Achievements.Commands.ClaimMyAchievement;

public record ClaimMyAchievementCommand(Guid AchievementId) : ICommand<ClaimMyAchievementResponse>;
