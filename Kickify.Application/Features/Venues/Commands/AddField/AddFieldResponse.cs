namespace Kickify.Application.Features.Venues.Commands.AddField
{
    public record AddFieldResponse(
        Guid FieldId,
        Guid VenueId,
        string Name,
        string FieldType,
        string? SurfaceType,
        decimal HourlyRate,
        decimal PeakHourSurcharge,
        TimeSpan? PeakStartTime,
        TimeSpan? PeakEndTime,
        decimal WeekendSurcharge,
        decimal HolidaySurcharge,
        List<Kickify.Domain.Enums.DayOfWeekEnum> PeakDaysOfWeek,
        bool IsPeakHourSurchargePercentage,
        bool IsWeekendSurchargePercentage,
        bool IsHolidaySurchargePercentage,
        DateTime CreatedAt
    );
}
