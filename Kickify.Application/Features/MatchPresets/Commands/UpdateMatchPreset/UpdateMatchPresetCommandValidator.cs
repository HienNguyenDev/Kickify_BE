using FluentValidation;

namespace Kickify.Application.Features.MatchPresets.Commands.UpdateMatchPreset
{
    public class UpdateMatchPresetCommandValidator : AbstractValidator<UpdateMatchPresetCommand>
    {
        public UpdateMatchPresetCommandValidator()
        {
            RuleFor(x => x.PresetId)
                .NotEmpty().WithMessage("Preset ID is required");

            RuleFor(x => x.PresetName)
                .MaximumLength(100).WithMessage("Preset name cannot exceed 100 characters")
                .When(x => x.PresetName != null);

            RuleFor(x => x.DurationMinutes)
                .GreaterThan(0).WithMessage("Duration must be greater than 0 minutes")
                .LessThanOrEqualTo(300).WithMessage("Duration cannot exceed 300 minutes")
                .When(x => x.DurationMinutes.HasValue);

            RuleFor(x => x.CustomLocation)
                .MaximumLength(200).WithMessage("Custom location cannot exceed 200 characters")
                .When(x => x.CustomLocation != null);

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters")
                .When(x => x.Description != null);
        }
    }
}
