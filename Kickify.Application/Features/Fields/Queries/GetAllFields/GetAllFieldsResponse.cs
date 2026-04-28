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
        decimal WeekendSurcharge,
        decimal HolidaySurcharge,
        bool IsActive,
        DateTime CreatedAt,
        List<FieldPeakHourResponseDto> PeakHours,
        bool IsWeekendSurchargePercentage,
        bool IsHolidaySurchargePercentage
    );

    public record FieldPeakHourResponseDto(
        Guid Id,
        TimeSpan StartTime,
        TimeSpan EndTime,
        decimal SurchargeAmount,
        bool IsPercentage,
        List<Kickify.Domain.Enums.DayOfWeekEnum> ApplicableDays
    );
}
