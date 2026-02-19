using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.Achievements.Queries.GetAllAchievements;

public record GetAllAchievementsQuery(
    int Page = 1,
    int PageSize = 10
) : IQuery<GetAllAchievementsResponse>;
