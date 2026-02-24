using FluentValidation;

namespace Kickify.Application.Features.Venues.Commands.DeleteVenue
{
    public class DeleteVenueCommandValidator : AbstractValidator<DeleteVenueCommand>
    {
        public DeleteVenueCommandValidator()
        {
            RuleFor(x => x.VenueId)
                .NotEmpty()
                .WithMessage("VenueId is required");
        }
    }
}
