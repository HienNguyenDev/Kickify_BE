using FluentValidation;

namespace Kickify.Application.Features.MatchPresets.Commands.CreateMatchPreset
{
    public class CreateMatchPresetCommandValidator : AbstractValidator<CreateMatchPresetCommand>
    {
        public CreateMatchPresetCommandValidator()
        {
            RuleFor(x => x.FieldId)
                .NotEmpty().WithMessage("Field ID is required");

            RuleFor(x => x.RoomName)
                .NotEmpty().WithMessage("Room name is required")
                .MaximumLength(100).WithMessage("Room name cannot exceed 100 characters");

            RuleFor(x => x.MatchFormat)
                .NotEmpty().WithMessage("Match format is required");

            RuleFor(x => x.DurationMinutes)
                .GreaterThan(0).WithMessage("Duration must be greater than 0 minutes")
                .LessThanOrEqualTo(300).WithMessage("Duration cannot exceed 300 minutes");

            RuleFor(x => x.Rules)
                .MaximumLength(1000).WithMessage("Rules cannot exceed 1000 characters")
                .When(x => x.Rules != null);

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");
        }
    }
}
