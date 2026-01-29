using FluentValidation;

namespace Kickify.Application.Features.MatchPresets.Commands.CreateMatchPreset
{
    public class CreateMatchPresetCommandValidator : AbstractValidator<CreateMatchPresetCommand>
    {
        public CreateMatchPresetCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required");

            RuleFor(x => x.PresetName)
                .NotEmpty().WithMessage("Preset name is required")
                .MaximumLength(100).WithMessage("Preset name cannot exceed 100 characters");

            RuleFor(x => x.MatchFormat)
                .NotEmpty().WithMessage("Match format is required");

            RuleFor(x => x.DurationMinutes)
                .GreaterThan(0).WithMessage("Duration must be greater than 0 minutes")
                .LessThanOrEqualTo(300).WithMessage("Duration cannot exceed 300 minutes");

            RuleFor(x => x.CustomLocation)
                .MaximumLength(200).WithMessage("Custom location cannot exceed 200 characters");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");
        }
    }
}
