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

    public record TimeSlotDto(
        TimeSpan StartTime,
        TimeSpan EndTime,
        bool IsAvailable,
        decimal PricePerHour
    );
}
