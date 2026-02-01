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
        List<UpdateVenueOperatingHourResponseDto> OperatingHours,
        DateTime UpdatedAt
    );

    public record UpdateVenueOperatingHourResponseDto(
        string DayOfWeek,
        TimeSpan? OpenTime,
        TimeSpan? CloseTime,
        bool IsClosed
    );
}
