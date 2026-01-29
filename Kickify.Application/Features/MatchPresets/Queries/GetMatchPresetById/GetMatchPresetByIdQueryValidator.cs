using FluentValidation;

namespace Kickify.Application.Features.MatchPresets.Queries.GetMatchPresetById
{
    public class GetMatchPresetByIdQueryValidator : AbstractValidator<GetMatchPresetByIdQuery>
    {
        public GetMatchPresetByIdQueryValidator()
        {
            RuleFor(x => x.PresetId)
                .NotEmpty().WithMessage("Preset ID is required");
        }
    }
}
