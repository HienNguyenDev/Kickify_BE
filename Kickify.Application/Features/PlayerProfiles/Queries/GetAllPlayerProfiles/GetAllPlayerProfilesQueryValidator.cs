using FluentValidation;

namespace Kickify.Application.Features.PlayerProfiles.Queries.GetAllPlayerProfiles
{
    public class GetAllPlayerProfilesQueryValidator : AbstractValidator<GetAllPlayerProfilesQuery>
    {
        public GetAllPlayerProfilesQueryValidator()
        {
            RuleFor(x => x.Page)
                .GreaterThanOrEqualTo(1).WithMessage("Page number must be at least 1");

            RuleFor(x => x.PageSize)
                .GreaterThanOrEqualTo(1).WithMessage("Page size must be at least 1")
                .LessThanOrEqualTo(100).WithMessage("Page size cannot exceed 100");

            RuleFor(x => x.MinElo)
                .GreaterThanOrEqualTo(0).WithMessage("Minimum ELO cannot be negative")
                .When(x => x.MinElo.HasValue);

            RuleFor(x => x.MaxElo)
                .GreaterThanOrEqualTo(0).WithMessage("Maximum ELO cannot be negative")
                .When(x => x.MaxElo.HasValue);

            RuleFor(x => x)
                .Must(x => x.MinElo <= x.MaxElo)
                .WithMessage("Minimum ELO must be less than or equal to Maximum ELO")
                .When(x => x.MinElo.HasValue && x.MaxElo.HasValue);

            RuleFor(x => x.MinTrustScore)
                .GreaterThanOrEqualTo(0).WithMessage("Minimum trust score cannot be negative")
                .LessThanOrEqualTo(100).WithMessage("Minimum trust score cannot exceed 100")
                .When(x => x.MinTrustScore.HasValue);
        }
    }
}
