using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.Fields.Commands.UpdateField
{
    public record UpdateFieldCommand(
        Guid FieldId,
        string? FieldName,
        string? FieldType,
        string? SurfaceType,
        decimal? HourlyRate,
        decimal? WeekendSurcharge,
        decimal? HolidaySurcharge,
        bool? IsActive,
            List<UpdateFieldPeakHourDto>? PeakHours,
        bool? IsWeekendSurchargePercentage,
        bool? IsHolidaySurchargePercentage
    ) : ICommand<UpdateFieldResponse>;

        public record UpdateFieldPeakHourDto(
            string StartTime,
            string EndTime,
            decimal SurchargeAmount,
            bool IsPercentage,
            List<string> ApplicableDays
        );
}
