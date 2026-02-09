using Kickify.Domain.Common;

namespace Kickify.Domain.Event;

public class MatchRoomCreatedDomainEvent : IDomainEvent
{
    public Guid RoomId { get; }

    public MatchRoomCreatedDomainEvent(Guid roomId)
    {
        RoomId = roomId;
    }
}
