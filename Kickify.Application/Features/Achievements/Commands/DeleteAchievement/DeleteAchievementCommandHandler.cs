using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace Kickify.Application.Features.Achievements.Commands.DeleteAchievement;

public class DeleteAchievementCommandHandler : ICommandHandler<DeleteAchievementCommand, DeleteAchievementResponse>
{
    private readonly IAchievementRepository _achievementRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteAchievementCommandHandler> _logger;

    public DeleteAchievementCommandHandler(
        IAchievementRepository achievementRepository,
        IUnitOfWork unitOfWork,
        ILogger<DeleteAchievementCommandHandler> logger)
    {
        _achievementRepository = achievementRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<DeleteAchievementResponse>> Handle(DeleteAchievementCommand request, CancellationToken cancellationToken)
    {
        var achievement = await _achievementRepository.GetByIdAsync(request.AchievementId, cancellationToken);
        if (achievement == null)
        {
            return Result.Failure<DeleteAchievementResponse>(AchievementErrors.NotFound(request.AchievementId));
        }

        // Soft delete via EF interceptor (sets DeletedAt in SaveChangesAsync)
        _achievementRepository.Remove(achievement);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Achievement soft-deleted: {AchievementId} - {Name}", achievement.AchievementId, achievement.Name);

        return Result.Success(new DeleteAchievementResponse(achievement.AchievementId, true));
    }
}
