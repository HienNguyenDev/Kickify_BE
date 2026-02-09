using Kickify.Domain.Common;

namespace Kickify.Domain.Event;

public class ParticipantJoinedRoomDomainEvent : IDomainEvent
{
    public Guid RoomId { get; }
    public string? OldAutoCloseJobId { get; }

    public ParticipantJoinedRoomDomainEvent(Guid roomId, string? oldAutoCloseJobId)
    {
        RoomId = roomId;
        OldAutoCloseJobId = oldAutoCloseJobId;
    }
}
