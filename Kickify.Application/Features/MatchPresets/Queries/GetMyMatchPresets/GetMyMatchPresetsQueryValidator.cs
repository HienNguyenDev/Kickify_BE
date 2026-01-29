using FluentValidation;

namespace Kickify.Application.Features.MatchPresets.Queries.GetMyMatchPresets
{
    public class GetMyMatchPresetsQueryValidator : AbstractValidator<GetMyMatchPresetsQuery>
    {
        public GetMyMatchPresetsQueryValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required");

            RuleFor(x => x.Page)
                .GreaterThanOrEqualTo(1).WithMessage("Page must be at least 1");

            RuleFor(x => x.PageSize)
                .GreaterThanOrEqualTo(1).WithMessage("Page size must be at least 1")
                .LessThanOrEqualTo(100).WithMessage("Page size cannot exceed 100");
        }
    }
}
