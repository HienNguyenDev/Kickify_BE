using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.Achievements.Queries.GetAchievementById;

public record GetAchievementByIdQuery(Guid AchievementId) : IQuery<GetAchievementByIdResponse>;
