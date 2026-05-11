using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kickify.Application.Features.Achievements.Queries.GetMyAchievements;

public class GetMyAchievementsQueryHandler : IQueryHandler<GetMyAchievementsQuery, GetMyAchievementsResponse>
{
    private readonly IAchievementRepository _achievementRepository;
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserContext _userContext;

    public GetMyAchievementsQueryHandler(
        IAchievementRepository achievementRepository,
        IApplicationDbContext dbContext,
        IUserContext userContext)
    {
        _achievementRepository = achievementRepository;
        _dbContext = dbContext;
        _userContext = userContext;
    }

    public async Task<Result<GetMyAchievementsResponse>> Handle(GetMyAchievementsQuery request, CancellationToken cancellationToken)
    {
        var userId = _userContext.UserId;
        var profile = await _dbContext.PlayerProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
        if (profile is null)
        {
            return Result.Failure<GetMyAchievementsResponse>(PlayerProfileErrors.NotFoundByUserId(userId));
        }

        var items = await _achievementRepository.GetAllWithUserProgressAsync(userId, cancellationToken);
        var feedbackGivenProgress = await _dbContext.MatchFeedbacks
            .AsNoTracking()
            .Where(x => x.ReviewerId == userId)
            .Select(x => x.RevieweeId)
            .Distinct()
            .CountAsync(cancellationToken);
        var receivedFeedbackProgress = await _dbContext.MatchFeedbacks
            .AsNoTracking()
            .CountAsync(x => x.RevieweeId == userId, cancellationToken);

        var achievements = items.Select(x =>
        {
            var targetValue = Math.Max(1, x.Achievement.CriteriaValue);
            var rawProgress = ResolveProgress(x.Achievement.CriteriaType, profile, feedbackGivenProgress, receivedFeedbackProgress);
            var currentProgress = Math.Min(rawProgress, targetValue);
            var progressPercent = Math.Round((decimal)currentProgress * 100m / targetValue, 2);
            var isUnlocked = x.EarnedAt.HasValue;
            var status = isUnlocked
                ? "Claimed"
                : rawProgress >= targetValue
                    ? "Claimable"
                    : "InProgress";

            return new MyAchievementItemDto(
                x.Achievement.AchievementId,
                x.Achievement.Name,
                x.Achievement.Description,
                x.Achievement.BadgeIconUrl,
                x.Achievement.CriteriaType.ToString(),
                targetValue,
                currentProgress,
                progressPercent,
                status,
                isUnlocked,
                x.EarnedAt
            );
        }).ToList();

        var unlockedCount = achievements.Count(a => a.IsUnlocked);

        var response = new GetMyAchievementsResponse(
            achievements,
            achievements.Count,
            unlockedCount
        );

        return Result.Success(response);
    }

    private static int ResolveProgress(
        CriteriaType criteriaType,
        Domain.Entities.PlayerProfile profile,
        int feedbackGivenProgress,
        int receivedFeedbackProgress)
    {
        return criteriaType switch
        {
            CriteriaType.Feedback => feedbackGivenProgress,
            CriteriaType.ReceivedFeedback => receivedFeedbackProgress,
            CriteriaType.Win => profile.Wins,
            CriteriaType.WinStreak => profile.MaxWinStreak,
            CriteriaType.Matches => profile.TotalMatches,
            CriteriaType.Fairplay => profile.TrustScore == 100 ? profile.TotalMatches : 0,
            _ => 0
        };
    }
}
