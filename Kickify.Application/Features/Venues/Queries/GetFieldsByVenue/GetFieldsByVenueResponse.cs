namespace Kickify.Application.Features.Venues.Queries.GetFieldsByVenue
{
    public record GetFieldsByVenueResponse(
        Guid VenueId,
        string VenueName,
        List<VenueFieldItemDto> Fields
    );

    public record VenueFieldItemDto(
        Guid FieldId,
        string FieldName,
        string FieldType,
        string? SurfaceType,
        decimal HourlyRate,
        decimal PeakHourSurcharge,
        TimeSpan? PeakStartTime,
        TimeSpan? PeakEndTime,
        decimal WeekendSurcharge,
        decimal HolidaySurcharge,
        bool IsActive,
        DateTime CreatedAt,
        List<Kickify.Domain.Enums.DayOfWeekEnum> PeakDaysOfWeek,
        bool IsPeakHourSurchargePercentage,
        bool IsWeekendSurchargePercentage,
        bool IsHolidaySurchargePercentage
    );
}
