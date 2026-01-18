namespace Kickify.Application.Features.Bookings.Queries.CheckConsecutiveSlots
{
    public record CheckConsecutiveSlotsResponse(
        Guid FieldId,
        string FieldName,
        DateTime Date,
        TimeSpan StartTime,
        TimeSpan EndTime,
        int DurationMinutes,
        bool IsAvailable,
        List<string> UnavailableSlots,
        string? Message,
        decimal HourlyRate,
        decimal TotalCost
    );
}
