namespace Kickify.Api.Requests;

public sealed record UpdateFieldRequest(
    string? Name,
    string? FieldType,
    string? SurfaceType,
    decimal? HourlyRate,
    decimal? WeekendSurcharge,
    decimal? HolidaySurcharge,
    bool? IsActive,
    List<FieldPeakHourDto>? PeakHours,
    bool? IsWeekendSurchargePercentage,
    bool? IsHolidaySurchargePercentage
);
