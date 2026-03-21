using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.Fields.Commands.UpdateField
{
    public record UpdateFieldCommand(
        Guid FieldId,
        string? FieldName,
        string? FieldType,
        string? SurfaceType,
        decimal? HourlyRate,
        decimal? PeakHourSurcharge,
        TimeSpan? PeakStartTime,
        TimeSpan? PeakEndTime,
        decimal? WeekendSurcharge,
        decimal? HolidaySurcharge,
        bool? IsActive,
        List<Kickify.Domain.Enums.DayOfWeekEnum>? PeakDaysOfWeek,
        bool? IsPeakHourSurchargePercentage,
        bool? IsWeekendSurchargePercentage,
        bool? IsHolidaySurchargePercentage
    ) : ICommand<UpdateFieldResponse>;
}
