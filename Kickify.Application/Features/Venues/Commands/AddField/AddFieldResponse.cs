namespace Kickify.Application.Features.Venues.Commands.AddField
{
    public record AddFieldResponse(
        Guid FieldId,
        Guid VenueId,
        string Name,
        string FieldType,
        string? SurfaceType,
        decimal HourlyRate,
        decimal WeekendSurcharge,
        decimal HolidaySurcharge,
        List<AddFieldPeakHourResponseDto> PeakHours,
        bool IsWeekendSurchargePercentage,
        bool IsHolidaySurchargePercentage,
        DateTime CreatedAt
    );

    public record AddFieldPeakHourResponseDto(
        Guid Id,
        TimeSpan StartTime,
        TimeSpan EndTime,
        decimal SurchargeAmount,
        bool IsPercentage,
        List<Kickify.Domain.Enums.DayOfWeekEnum> ApplicableDays
    );
}
