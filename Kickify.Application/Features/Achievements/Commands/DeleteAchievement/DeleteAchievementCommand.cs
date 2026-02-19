using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.Achievements.Commands.DeleteAchievement;

public record DeleteAchievementCommand(Guid AchievementId) : ICommand<DeleteAchievementResponse>;
