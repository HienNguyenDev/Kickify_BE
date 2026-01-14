using FluentValidation;

namespace Kickify.Application.Features.Venues.Queries.GetVenueById
{
    public class GetVenueByIdQueryValidator : AbstractValidator<GetVenueByIdQuery>
    {
        public GetVenueByIdQueryValidator()
        {
            RuleFor(x => x.VenueId)
                .NotEmpty().WithMessage("VenueId is required");
        }
    }
}
