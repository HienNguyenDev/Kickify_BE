using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Abstractions.Services;
using Kickify.Domain.Common;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace Kickify.Application.Features.Achievements.Commands.UpdateAchievement;

public class UpdateAchievementCommandHandler : ICommandHandler<UpdateAchievementCommand, UpdateAchievementResponse>
{
    private readonly IAchievementRepository _achievementRepository;
    private readonly IStorageService _storageService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateAchievementCommandHandler> _logger;

    public UpdateAchievementCommandHandler(
        IAchievementRepository achievementRepository,
        IStorageService storageService,
        IUnitOfWork unitOfWork,
        ILogger<UpdateAchievementCommandHandler> logger)
    {
        _achievementRepository = achievementRepository;
        _storageService = storageService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<UpdateAchievementResponse>> Handle(UpdateAchievementCommand request, CancellationToken cancellationToken)
    {
        // Fetch existing
        var achievement = await _achievementRepository.GetByIdAsync(request.AchievementId, cancellationToken);
        if (achievement == null)
        {
            return Result.Failure<UpdateAchievementResponse>(AchievementErrors.NotFound(request.AchievementId));
        }

        // Check duplicate name (exclude self)
        var nameExists = await _achievementRepository.ExistsByNameAsync(request.Name, request.AchievementId, cancellationToken);
        if (nameExists)
        {
            return Result.Failure<UpdateAchievementResponse>(AchievementErrors.NameAlreadyExists);
        }

        // Parse criteria type
        if (!Enum.TryParse<CriteriaType>(request.CriteriaType, true, out var criteriaType))
        {
            return Result.Failure<UpdateAchievementResponse>(AchievementErrors.InvalidCriteriaType);
        }

        // Upload new icon if provided
        if (request.IconFile != null)
        {
            // Delete old icon if exists
            if (!string.IsNullOrEmpty(achievement.BadgeIconUrl))
            {
                var oldObjectName = ExtractObjectName(achievement.BadgeIconUrl);
                if (!string.IsNullOrEmpty(oldObjectName))
                {
                    await _storageService.DeleteAsync(oldObjectName, cancellationToken);
                }
            }

            var uploadResult = await _storageService.UploadAsync(
                request.IconFile.Stream,
                request.IconFile.FileName,
                request.IconFile.ContentType,
                cancellationToken);

            if (!uploadResult.Success)
            {
                return Result.Failure<UpdateAchievementResponse>(AchievementErrors.IconUploadFailed);
            }

            achievement.BadgeIconUrl = uploadResult.PublicUrl;
        }

        // Update fields
        achievement.Name = request.Name;
        achievement.Description = request.Description;
        achievement.CriteriaType = criteriaType;
        achievement.CriteriaValue = request.CriteriaValue;

        _achievementRepository.Update(achievement);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Achievement updated: {AchievementId} - {Name}", achievement.AchievementId, achievement.Name);

        return Result.Success(new UpdateAchievementResponse(
            achievement.AchievementId,
            achievement.Name,
            achievement.Description,
            achievement.BadgeIconUrl,
            achievement.CriteriaType.ToString(),
            achievement.CriteriaValue,
            achievement.UpdatedAt
        ));
    }

    /// <summary>
    /// Extract object name from MinIO public URL for deletion
    /// </summary>
    private static string? ExtractObjectName(string publicUrl)
    {
        try
        {
            var uri = new Uri(publicUrl);
            // Remove leading slash and bucket name
            var pathSegments = uri.AbsolutePath.TrimStart('/').Split('/', 2);
            return pathSegments.Length > 1 ? pathSegments[1] : null;
        }
        catch
        {
            return null;
        }
    }
}
