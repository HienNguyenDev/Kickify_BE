namespace Kickify.Api.Requests;

public sealed record UpdateFieldRequest(
    string? Name,
    string? FieldType,
    string? SurfaceType,
    decimal? HourlyRate,
    decimal? PeakHourSurcharge,
    bool? IsActive
);
