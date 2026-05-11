namespace Kickify.Application.Features.Venues.Commands.CreateVenue
{
    public record CreateVenueResponse(
        Guid VenueId,
        string Name,
        string Address,
        decimal Latitude,
        decimal Longitude,
        string? ContactPhone,
        string? ContactEmail,
        string? Description,
        string? Amenities,
        Guid WalletId,
        List<VenueFieldDto> Fields,
        DateTime CreatedAt
    );

    public record VenueFieldDto(
        Guid FieldId,
        string Name,
        string FieldType,
        string? SurfaceType,
        decimal HourlyRate,
        decimal WeekendSurcharge,
        decimal HolidaySurcharge,
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
