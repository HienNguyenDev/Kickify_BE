namespace Kickify.Application.Features.Venues.Commands.CreateVenue
{
    public record CreateVenueResponse(
        Guid VenueId,
        string Name,
        string Address,
        decimal Latitude,
        decimal Longitude,
        string? ContactPhone,
        string? ContactEmail,
        string? Description,
        string? Amenities,
        Guid WalletId,
        List<VenueFieldDto> Fields,
        DateTime CreatedAt
    );

    public record VenueFieldDto(
        Guid FieldId,
        string Name,
        string FieldType,
        int MaxPlayers,
        decimal PricePerHour,
        string? Description
    );
}
