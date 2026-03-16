namespace Kickify.Api.Requests;

public sealed record UpdateFieldRequest(
    string? Name,
    string? FieldType,
    string? SurfaceType,
    decimal? HourlyRate,
    decimal? PeakHourSurcharge,
    TimeSpan? PeakStartTime,
    TimeSpan? PeakEndTime,
    decimal? WeekendSurcharge,
    decimal? HolidaySurcharge,
    bool? IsActive
);
