namespace Kickify.Application.Features.Venues.Commands.AddField
{
    public record AddFieldResponse(
        Guid FieldId,
        Guid VenueId,
        string Name,
        string FieldType,
        string? SurfaceType,
        decimal HourlyRate,
        decimal PeakHourSurcharge,
        TimeSpan? PeakStartTime,
        TimeSpan? PeakEndTime,
        decimal WeekendSurcharge,
        decimal HolidaySurcharge,
        DateTime CreatedAt
    );
}
