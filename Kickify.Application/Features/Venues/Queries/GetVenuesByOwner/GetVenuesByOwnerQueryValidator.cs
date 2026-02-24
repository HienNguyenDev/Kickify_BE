using FluentValidation;

namespace Kickify.Application.Features.Venues.Queries.GetVenuesByOwner
{
    public class GetVenuesByOwnerQueryValidator : AbstractValidator<GetVenuesByOwnerQuery>
    {
        public GetVenuesByOwnerQueryValidator()
        {
            RuleFor(x => x.Page)
                .GreaterThan(0)
                .WithMessage("Page must be greater than 0");

            RuleFor(x => x.PageSize)
                .GreaterThan(0)
                .LessThanOrEqualTo(100)
                .WithMessage("PageSize must be between 1 and 100");
        }
    }
}
