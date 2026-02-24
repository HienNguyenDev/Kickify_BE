namespace Kickify.Application.Features.Venues.Commands.DeleteVenue
{
    public record DeleteVenueResponse(
        Guid VenueId,
        bool IsDeleted
    );
}
