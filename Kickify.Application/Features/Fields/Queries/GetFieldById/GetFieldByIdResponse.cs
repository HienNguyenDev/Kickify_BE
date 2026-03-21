namespace Kickify.Application.Features.Fields.Queries.GetFieldById
{
    public record GetFieldByIdResponse(
        Guid FieldId,
        Guid VenueId,
        string VenueName,
        string VenueAddress,
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
        List<OperatingHourDto> OperatingHours,
        DateTime CreatedAt,
        List<Kickify.Domain.Enums.DayOfWeekEnum> PeakDaysOfWeek,
        bool IsPeakHourSurchargePercentage,
        bool IsWeekendSurchargePercentage,
        bool IsHolidaySurchargePercentage
    );

    public record OperatingHourDto(
        DayOfWeek DayOfWeek,
        TimeSpan OpenTime,
        TimeSpan CloseTime,
        bool IsClosed
    );
}
