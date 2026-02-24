namespace Kickify.Application.Features.Bookings.Queries.CheckAvailability
{
    public record CheckAvailabilityResponse(
        Guid FieldId,
        string FieldName,
        DateTime Date,
        TimeSpan? OpenTime,
        TimeSpan? CloseTime,
        List<TimeSlotDto> TimeSlots,
        string? Message
    );

    /// <summary>
    /// Represents a 30-minute time slot
    /// </summary>
    public record TimeSlotDto(
        TimeSpan StartTime,
        bool IsAvailable,
        decimal HourlyRate
    );
}
