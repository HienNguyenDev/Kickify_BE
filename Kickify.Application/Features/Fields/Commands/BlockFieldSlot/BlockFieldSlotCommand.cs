using Kickify.Domain.Common;
using MediatR;

namespace Kickify.Application.Features.Fields.Commands.BlockFieldSlot
{
    /// <summary>
    /// Command to block a time slot on a field (Ghost Booking pattern)
    /// Used by venue owners to mark slots as unavailable for maintenance or offline bookings
    /// </summary>
    public record BlockFieldSlotCommand(
        Guid VenueId,
        Guid FieldId,
        Guid UserId,
        DateTime Date,
        TimeSpan StartTime,
        TimeSpan EndTime,
        string Reason,
        decimal Amount = 0
    ) : IRequest<Result<BlockFieldSlotResponse>>;

    public record BlockFieldSlotResponse(
        Guid RoomId,
        Guid BookingId,
        Guid FieldId,
        DateTime Date,
        TimeSpan StartTime,
        TimeSpan EndTime,
        string Reason,
        decimal Amount,
        string TransactionReference,
        DateTime CreatedAt
    );
}
