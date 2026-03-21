namespace Kickify.Api.Requests;

public sealed record UpdateVenueRequest(
    string? Name,
    string? Address,
    decimal? Latitude,
    decimal? Longitude,
    string? ContactPhone,
    string? ContactEmail,
    string? Description,
    string? Amenities,
    List<Guid>? IgnoredHolidayIds,
    List<UpdateVenueOperatingHourDto>? OperatingHours
);

public record UpdateVenueOperatingHourDto(
    int DayOfWeek,
    string? OpenTime,
    string? CloseTime
);
