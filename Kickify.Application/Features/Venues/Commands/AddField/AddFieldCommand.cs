using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.Venues.Commands.AddField
{
    public record AddFieldCommand(
          Guid VenueId,
          string Name,
          string FieldType,
          string? SurfaceType,
          decimal HourlyRate,
          decimal WeekendSurcharge,
          decimal HolidaySurcharge,
          List<AddFieldPeakHourDto>? PeakHours,
          bool? IsWeekendSurchargePercentage,
          bool? IsHolidaySurchargePercentage
      ) : ICommand<AddFieldResponse>;

    public record AddFieldPeakHourDto(
        string StartTime,
        string EndTime,
        decimal SurchargeAmount,
        bool IsPercentage,
        List<string> ApplicableDays
    );
}
