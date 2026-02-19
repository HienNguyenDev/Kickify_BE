using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;

namespace Kickify.Application.Features.Achievements.Queries.GetMyAchievements;

public class GetMyAchievementsQueryHandler : IQueryHandler<GetMyAchievementsQuery, GetMyAchievementsResponse>
{
    private readonly IAchievementRepository _achievementRepository;
    private readonly IUserContext _userContext;

    public GetMyAchievementsQueryHandler(
        IAchievementRepository achievementRepository,
        IUserContext userContext)
    {
        _achievementRepository = achievementRepository;
        _userContext = userContext;
    }

    public async Task<Result<GetMyAchievementsResponse>> Handle(GetMyAchievementsQuery request, CancellationToken cancellationToken)
    {
        var userId = _userContext.UserId;

        var items = await _achievementRepository.GetAllWithUserProgressAsync(userId, cancellationToken);

        var achievements = items.Select(x => new MyAchievementItemDto(
            x.Achievement.AchievementId,
            x.Achievement.Name,
            x.Achievement.Description,
            x.Achievement.BadgeIconUrl,
            x.Achievement.CriteriaType.ToString(),
            x.Achievement.CriteriaValue,
            x.EarnedAt.HasValue,
            x.EarnedAt
        )).ToList();

        var unlockedCount = achievements.Count(a => a.IsUnlocked);

        var response = new GetMyAchievementsResponse(
            achievements,
            achievements.Count,
            unlockedCount
        );

        return Result.Success(response);
    }
}
