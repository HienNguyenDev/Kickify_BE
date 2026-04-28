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
        List<VenueFieldPeakHourResponseDto> PeakHours,
        decimal WeekendSurcharge,
        decimal HolidaySurcharge,
        bool IsActive,
        DateTime CreatedAt,
        bool IsWeekendSurchargePercentage,
        bool IsHolidaySurchargePercentage
    );

    public record VenueFieldPeakHourResponseDto(
        Guid Id,
        TimeSpan StartTime,
        TimeSpan EndTime,
        decimal SurchargeAmount,
        bool IsPercentage,
        List<Kickify.Domain.Enums.DayOfWeekEnum> ApplicableDays
    );
}
