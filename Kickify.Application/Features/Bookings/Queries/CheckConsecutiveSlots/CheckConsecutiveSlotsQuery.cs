using Kickify.Domain.Common;
using MediatR;

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
    ) : IRequest<Result<CheckConsecutiveSlotsResponse>>;
}
