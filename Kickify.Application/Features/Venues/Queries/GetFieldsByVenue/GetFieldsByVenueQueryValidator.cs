using FluentValidation;

namespace Kickify.Application.Features.Venues.Queries.GetFieldsByVenue
{
    public class GetFieldsByVenueQueryValidator : AbstractValidator<GetFieldsByVenueQuery>
    {
        public GetFieldsByVenueQueryValidator()
        {
            RuleFor(x => x.VenueId)
                .NotEmpty()
                .WithMessage("VenueId is required");
        }
    }
}
