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
        decimal WeekendSurcharge,
        decimal HolidaySurcharge,
        bool IsActive,
        DateTime CreatedAt,
        List<OwnerFieldPeakHourResponseDto> PeakHours,
        bool IsWeekendSurchargePercentage,
        bool IsHolidaySurchargePercentage
    );

    public record OwnerFieldPeakHourResponseDto(
        Guid Id,
        TimeSpan StartTime,
        TimeSpan EndTime,
        decimal SurchargeAmount,
        bool IsPercentage,
        List<Kickify.Domain.Enums.DayOfWeekEnum> ApplicableDays
    );
}
