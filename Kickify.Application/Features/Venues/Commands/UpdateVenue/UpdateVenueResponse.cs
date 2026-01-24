namespace Kickify.Application.Features.Venues.Commands.UpdateVenue
{
    public record UpdateVenueResponse(
        Guid VenueId,
        string Name,
        string Address,
        decimal? Latitude,
        decimal? Longitude,
        string? ContactPhone,
        string? ContactEmail,
        string? Description,
        string? Amenities,
        DateTime UpdatedAt
    );
}
