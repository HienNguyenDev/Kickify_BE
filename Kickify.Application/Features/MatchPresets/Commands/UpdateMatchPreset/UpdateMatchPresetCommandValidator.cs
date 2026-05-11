using FluentValidation;

namespace Kickify.Application.Features.MatchPresets.Commands.UpdateMatchPreset
{
    public class UpdateMatchPresetCommandValidator : AbstractValidator<UpdateMatchPresetCommand>
    {
        public UpdateMatchPresetCommandValidator()
        {
            RuleFor(x => x.PresetId)
                .NotEmpty().WithMessage("Preset ID is required");

            RuleFor(x => x.FieldId)
                .NotEmpty().WithMessage("Field ID cannot be empty")
                .When(x => x.FieldId.HasValue);

            RuleFor(x => x.RoomName)
                .MaximumLength(100).WithMessage("Room name cannot exceed 100 characters")
                .When(x => x.RoomName != null);

            RuleFor(x => x.DurationMinutes)
                .GreaterThan(0).WithMessage("Duration must be greater than 0 minutes")
                .LessThanOrEqualTo(300).WithMessage("Duration cannot exceed 300 minutes")
                .When(x => x.DurationMinutes.HasValue);

            RuleFor(x => x.Rules)
                .MaximumLength(1000).WithMessage("Rules cannot exceed 1000 characters")
                .When(x => x.Rules != null);

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters")
                .When(x => x.Description != null);
        }
    }
}
