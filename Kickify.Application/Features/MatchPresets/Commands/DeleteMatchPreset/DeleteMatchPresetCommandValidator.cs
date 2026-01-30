using FluentValidation;

namespace Kickify.Application.Features.MatchPresets.Commands.DeleteMatchPreset
{
    public class DeleteMatchPresetCommandValidator : AbstractValidator<DeleteMatchPresetCommand>
    {
        public DeleteMatchPresetCommandValidator()
        {
            RuleFor(x => x.PresetId)
                .NotEmpty().WithMessage("Preset ID is required");
        }
    }
}
