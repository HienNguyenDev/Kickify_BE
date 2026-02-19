using FluentValidation;
using Kickify.Domain.Enums;

namespace Kickify.Application.Features.Achievements.Commands.CreateAchievement;

public class CreateAchievementCommandValidator : AbstractValidator<CreateAchievementCommand>
{
    private static readonly string[] AllowedImageTypes = ["image/jpeg", "image/png", "image/gif", "image/webp"];
    private const long MaxFileSize = 5 * 1024 * 1024; // 5MB

    public CreateAchievementCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters");

        RuleFor(x => x.CriteriaType)
            .NotEmpty().WithMessage("CriteriaType is required")
            .Must(BeValidCriteriaType).WithMessage("Invalid CriteriaType. Allowed: Feedback, WinStreak, Fairplay, Matches, Other");

        RuleFor(x => x.CriteriaValue)
            .GreaterThanOrEqualTo(0).WithMessage("CriteriaValue must be >= 0");

        When(x => x.IconFile != null, () =>
        {
            RuleFor(x => x.IconFile!.FileSize)
                .LessThanOrEqualTo(MaxFileSize)
                .WithMessage($"Icon file size must not exceed {MaxFileSize / (1024 * 1024)}MB");

            RuleFor(x => x.IconFile!.ContentType)
                .Must(ct => AllowedImageTypes.Contains(ct))
                .WithMessage("Invalid file type. Allowed: JPEG, PNG, GIF, WebP");
        });
    }

    private static bool BeValidCriteriaType(string criteriaType)
    {
        return Enum.TryParse<CriteriaType>(criteriaType, true, out _);
    }
}
