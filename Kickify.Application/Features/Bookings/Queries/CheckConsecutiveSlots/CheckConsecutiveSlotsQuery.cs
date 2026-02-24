using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.Bookings.Queries.CheckConsecutiveSlots
{
    /// <summary>
    /// Check if consecutive time slots are available for a given duration
    /// </summary>
    public record CheckConsecutiveSlotsQuery(
        Guid FieldId,
        DateTime Date,
        TimeSpan StartTime,
        int DurationMinutes
    ) : IQuery<CheckConsecutiveSlotsResponse>;
}
