using FluentValidation;

namespace Kickify.Application.Features.Venues.Queries.GetAllVenues
{
    public class GetAllVenuesQueryValidator : AbstractValidator<GetAllVenuesQuery>
    {
        public GetAllVenuesQueryValidator()
        {
            RuleFor(x => x.Page)
                .GreaterThan(0).WithMessage("Page must be greater than 0");

            RuleFor(x => x.PageSize)
                .GreaterThan(0).WithMessage("PageSize must be greater than 0")
                .LessThanOrEqualTo(100).WithMessage("PageSize must not exceed 100");

            When(x => x.Latitude.HasValue, () =>
            {
                RuleFor(x => x.Latitude)
                    .InclusiveBetween(-90, 90).WithMessage("Latitude must be between -90 and 90");
            });

            When(x => x.Longitude.HasValue, () =>
            {
                RuleFor(x => x.Longitude)
                    .InclusiveBetween(-180, 180).WithMessage("Longitude must be between -180 and 180");
            });

            When(x => x.RadiusKm.HasValue, () =>
            {
                RuleFor(x => x.RadiusKm)
                    .GreaterThan(0).WithMessage("RadiusKm must be greater than 0");
            });
        }
    }
}
