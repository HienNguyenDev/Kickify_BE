namespace Kickify.Application.Features.Fields.Queries.GetAllFields
{
    public record GetAllFieldsResponse(
        List<FieldItemDto> Fields,
        int TotalCount,
        int Page,
        int PageSize,
        int TotalPages
    );

    public record FieldItemDto(
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
        DateTime CreatedAt,
        List<Kickify.Domain.Enums.DayOfWeekEnum> PeakDaysOfWeek,
        bool IsPeakHourSurchargePercentage,
        bool IsWeekendSurchargePercentage,
        bool IsHolidaySurchargePercentage
    );
}
