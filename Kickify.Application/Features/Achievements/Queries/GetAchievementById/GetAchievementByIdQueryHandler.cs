using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.Achievements.Queries.GetAchievementById;

public class GetAchievementByIdQueryHandler : IQueryHandler<GetAchievementByIdQuery, GetAchievementByIdResponse>
{
    private readonly IAchievementRepository _achievementRepository;

    public GetAchievementByIdQueryHandler(IAchievementRepository achievementRepository)
    {
        _achievementRepository = achievementRepository;
    }

    public async Task<Result<GetAchievementByIdResponse>> Handle(GetAchievementByIdQuery request, CancellationToken cancellationToken)
    {
        var achievement = await _achievementRepository.GetByIdAsync(request.AchievementId, cancellationToken);
        if (achievement == null)
        {
            return Result.Failure<GetAchievementByIdResponse>(AchievementErrors.NotFound(request.AchievementId));
        }

        var response = new GetAchievementByIdResponse(
            achievement.AchievementId,
            achievement.Name,
            achievement.Description,
            achievement.BadgeIconUrl,
            achievement.CriteriaType.ToString(),
            achievement.CriteriaValue,
            achievement.CreatedAt,
            achievement.UpdatedAt
        );

        return Result.Success(response);
    }
}
