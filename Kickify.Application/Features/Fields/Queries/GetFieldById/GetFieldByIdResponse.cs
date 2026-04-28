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
        decimal WeekendSurcharge,
        decimal HolidaySurcharge,
        bool IsActive,
        List<OperatingHourDto> OperatingHours,
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

    public record OperatingHourDto(
        DayOfWeek DayOfWeek,
        TimeSpan OpenTime,
        TimeSpan CloseTime,
        bool IsClosed
    );
}
