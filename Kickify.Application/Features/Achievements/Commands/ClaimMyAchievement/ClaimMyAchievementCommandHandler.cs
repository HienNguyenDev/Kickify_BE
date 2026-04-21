using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kickify.Application.Features.Achievements.Commands.ClaimMyAchievement;

public class ClaimMyAchievementCommandHandler : ICommandHandler<ClaimMyAchievementCommand, ClaimMyAchievementResponse>
{
    private readonly IAchievementRepository _achievementRepository;
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserContext _userContext;
    private readonly IUnitOfWork _unitOfWork;

    public ClaimMyAchievementCommandHandler(
        IAchievementRepository achievementRepository,
        IApplicationDbContext dbContext,
        IUserContext userContext,
        IUnitOfWork unitOfWork)
    {
        _achievementRepository = achievementRepository;
        _dbContext = dbContext;
        _userContext = userContext;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ClaimMyAchievementResponse>> Handle(ClaimMyAchievementCommand request, CancellationToken cancellationToken)
    {
        var userId = _userContext.UserId;
        var achievement = await _achievementRepository.GetByIdAsync(request.AchievementId, cancellationToken);
        if (achievement is null)
        {
            return Result.Failure<ClaimMyAchievementResponse>(AchievementErrors.NotFound(request.AchievementId));
        }

        var alreadyClaimed = await _achievementRepository.HasUserClaimedAsync(userId, request.AchievementId, cancellationToken);
        if (alreadyClaimed)
        {
            return Result.Failure<ClaimMyAchievementResponse>(AchievementErrors.AlreadyClaimed);
        }

        var profile = await _dbContext.PlayerProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
        if (profile is null)
        {
            return Result.Failure<ClaimMyAchievementResponse>(PlayerProfileErrors.NotFoundByUserId(userId));
        }

        var feedbackGivenProgress = await _dbContext.MatchFeedbacks
            .AsNoTracking()
            .Where(x => x.ReviewerId == userId)
            .Select(x => x.RevieweeId)
            .Distinct()
            .CountAsync(cancellationToken);
        var receivedFeedbackProgress = await _dbContext.MatchFeedbacks
            .AsNoTracking()
            .CountAsync(x => x.RevieweeId == userId, cancellationToken);

        var targetValue = Math.Max(1, achievement.CriteriaValue);
        var currentProgress = ResolveProgress(achievement.CriteriaType, profile, feedbackGivenProgress, receivedFeedbackProgress);
        if (currentProgress < targetValue)
        {
            return Result.Failure<ClaimMyAchievementResponse>(AchievementErrors.ClaimConditionNotMet(achievement.Name));
        }

        var earnedAt = DateTime.UtcNow;
        var playerAchievement = new PlayerAchievement
        {
            PlayerAchievementId = Guid.NewGuid(),
            UserId = userId,
            AchievementId = achievement.AchievementId,
            EarnedAt = earnedAt
        };

        await _achievementRepository.AddPlayerAchievementAsync(playerAchievement, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new ClaimMyAchievementResponse(
            achievement.AchievementId,
            achievement.Name,
            earnedAt,
            "Claimed"));
    }

    private static int ResolveProgress(
        CriteriaType criteriaType,
        PlayerProfile profile,
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
