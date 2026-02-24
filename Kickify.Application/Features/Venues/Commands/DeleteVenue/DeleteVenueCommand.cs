using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.Venues.Commands.DeleteVenue
{
    public record DeleteVenueCommand(
        Guid VenueId
    ) : ICommand<DeleteVenueResponse>;
}
