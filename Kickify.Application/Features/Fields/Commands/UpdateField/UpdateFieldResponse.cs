namespace Kickify.Application.Features.Fields.Commands.UpdateField
{
    public record UpdateFieldResponse(
        Guid FieldId,
        Guid VenueId,
        string FieldName,
        string FieldType,
        string? SurfaceType,
        decimal HourlyRate,
        decimal WeekendSurcharge,
        decimal HolidaySurcharge,
            List<UpdateFieldPeakHourResponseDto> PeakHours,
        bool IsWeekendSurchargePercentage,
        bool IsHolidaySurchargePercentage,
        bool IsActive,
        DateTime UpdatedAt
    );

        public record UpdateFieldPeakHourResponseDto(
            Guid Id,
            TimeSpan StartTime,
            TimeSpan EndTime,
            decimal SurchargeAmount,
            bool IsPercentage,
            List<Kickify.Domain.Enums.DayOfWeekEnum> ApplicableDays
        );
}
