using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;

namespace Kickify.Application.Features.Achievements.Queries.GetAllAchievements;

public class GetAllAchievementsQueryHandler : IQueryHandler<GetAllAchievementsQuery, GetAllAchievementsResponse>
{
    private readonly IAchievementRepository _achievementRepository;

    public GetAllAchievementsQueryHandler(IAchievementRepository achievementRepository)
    {
        _achievementRepository = achievementRepository;
    }

    public async Task<Result<GetAllAchievementsResponse>> Handle(GetAllAchievementsQuery request, CancellationToken cancellationToken)
    {
        var (items, total) = await _achievementRepository.GetAllPagedAsync(request.Page, request.PageSize, cancellationToken);

        var achievements = items.Select(a => new AchievementItemDto(
            a.AchievementId,
            a.Name,
            a.Description,
            a.BadgeIconUrl,
            a.CriteriaType.ToString(),
            a.CriteriaValue,
            a.CreatedAt,
            a.UpdatedAt
        )).ToList();

        var response = new GetAllAchievementsResponse(
            achievements,
            total,
            request.Page,
            request.PageSize,
            (int)Math.Ceiling(total / (double)request.PageSize)
        );

        return Result.Success(response);
    }
}
