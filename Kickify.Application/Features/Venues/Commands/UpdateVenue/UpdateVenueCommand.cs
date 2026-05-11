using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.Venues.Commands.UpdateVenue
{
    public record UpdateVenueCommand(
        Guid VenueId,
        string? Name,
        string? Address,
        decimal? Latitude,
        decimal? Longitude,
        string? ContactPhone,
        string? ContactEmail,
        string? Description,
        string? Amenities,
        List<Guid>? IgnoredHolidayIds,
        List<UpdateVenueOperatingHourItemDto>? OperatingHours
    ) : ICommand<UpdateVenueResponse>;

    public record UpdateVenueOperatingHourItemDto(
        int DayOfWeek,
        string? OpenTime,
        string? CloseTime
    );
}
