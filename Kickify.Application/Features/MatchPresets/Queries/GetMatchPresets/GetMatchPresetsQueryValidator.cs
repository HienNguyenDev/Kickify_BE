using FluentValidation;

namespace Kickify.Application.Features.MatchPresets.Queries.GetMatchPresets
{
    public class GetMatchPresetsQueryValidator : AbstractValidator<GetMatchPresetsQuery>
    {
        public GetMatchPresetsQueryValidator()
        {
            RuleFor(x => x.Page)
                .GreaterThanOrEqualTo(1).WithMessage("Page must be at least 1");

            RuleFor(x => x.PageSize)
                .GreaterThanOrEqualTo(1).WithMessage("Page size must be at least 1")
                .LessThanOrEqualTo(100).WithMessage("Page size cannot exceed 100");
        }
    }
}
