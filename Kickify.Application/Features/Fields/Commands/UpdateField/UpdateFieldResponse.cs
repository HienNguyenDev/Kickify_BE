namespace Kickify.Application.Features.Fields.Commands.UpdateField
{
    public record UpdateFieldResponse(
        Guid FieldId,
        Guid VenueId,
        string FieldName,
        string FieldType,
        string? SurfaceType,
        decimal HourlyRate,
        decimal PeakHourSurcharge,
        TimeSpan? PeakStartTime,
        TimeSpan? PeakEndTime,
        decimal WeekendSurcharge,
        decimal HolidaySurcharge,
        bool IsActive,
        DateTime UpdatedAt
    );
}
