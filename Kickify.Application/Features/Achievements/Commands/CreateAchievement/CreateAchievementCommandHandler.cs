using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Abstractions.Services;
using Kickify.Domain.Common;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace Kickify.Application.Features.Achievements.Commands.CreateAchievement;

public class CreateAchievementCommandHandler : ICommandHandler<CreateAchievementCommand, CreateAchievementResponse>
{
    private readonly IAchievementRepository _achievementRepository;
    private readonly IStorageService _storageService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateAchievementCommandHandler> _logger;

    public CreateAchievementCommandHandler(
        IAchievementRepository achievementRepository,
        IStorageService storageService,
        IUnitOfWork unitOfWork,
        ILogger<CreateAchievementCommandHandler> logger)
    {
        _achievementRepository = achievementRepository;
        _storageService = storageService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<CreateAchievementResponse>> Handle(CreateAchievementCommand request, CancellationToken cancellationToken)
    {
        // Check duplicate name
        var nameExists = await _achievementRepository.ExistsByNameAsync(request.Name, cancellationToken: cancellationToken);
        if (nameExists)
        {
            return Result.Failure<CreateAchievementResponse>(AchievementErrors.NameAlreadyExists);
        }

        // Parse criteria type
        if (!Enum.TryParse<CriteriaType>(request.CriteriaType, true, out var criteriaType))
        {
            return Result.Failure<CreateAchievementResponse>(AchievementErrors.InvalidCriteriaType);
        }

        // Upload icon if provided
        string? badgeIconUrl = null;
        if (request.IconFile != null)
        {
            var uploadResult = await _storageService.UploadAsync(
                request.IconFile.Stream,
                request.IconFile.FileName,
                request.IconFile.ContentType,
                cancellationToken);

            if (!uploadResult.Success)
            {
                return Result.Failure<CreateAchievementResponse>(AchievementErrors.IconUploadFailed);
            }

            badgeIconUrl = uploadResult.PublicUrl;
        }

        // Create entity
        var achievement = new Achievement
        {
            AchievementId = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            BadgeIconUrl = badgeIconUrl,
            CriteriaType = criteriaType,
            CriteriaValue = request.CriteriaValue
        };

        await _achievementRepository.AddAsync(achievement);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Achievement created: {AchievementId} - {Name}", achievement.AchievementId, achievement.Name);

        return Result.Success(new CreateAchievementResponse(
            achievement.AchievementId,
            achievement.Name,
            achievement.Description,
            achievement.BadgeIconUrl,
            achievement.CriteriaType.ToString(),
            achievement.CriteriaValue,
            achievement.CreatedAt
        ));
    }
}
