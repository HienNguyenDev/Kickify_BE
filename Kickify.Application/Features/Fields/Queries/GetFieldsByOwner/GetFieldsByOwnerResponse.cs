namespace Kickify.Application.Features.Fields.Queries.GetFieldsByOwner
{
    public record GetFieldsByOwnerResponse(
        List<OwnerFieldItemDto> Fields,
        int Total,
        int Page,
        int PageSize
    );

    public record OwnerFieldItemDto(
        Guid FieldId,
        Guid VenueId,
        string VenueName,
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
