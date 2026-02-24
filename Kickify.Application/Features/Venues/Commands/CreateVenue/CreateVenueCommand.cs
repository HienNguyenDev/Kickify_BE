using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.Venues.Commands.CreateVenue
{
    public record CreateVenueCommand(
        string Name,
        string Address,
        decimal Latitude,
        decimal Longitude,
        string? ContactPhone,
        string? ContactEmail,
        string? Description,
        string? Amenities,
        List<CreateVenueFieldDto> Fields,
        List<CreateVenueOperatingHoursDto> OperatingHours
    ) : ICommand<CreateVenueResponse>;

    public record CreateVenueFieldDto(
        string Name,
        string FieldType,
        string? SurfaceType,
        decimal HourlyRate,
        decimal PeakHourSurcharge
     );

    public record CreateVenueOperatingHoursDto(
        int DayOfWeek,
        TimeSpan OpenTime,
        TimeSpan CloseTime
    );
}
