using Kickify.Domain.Common;

namespace Kickify.Domain.Event;

/// <summary>Host set the room to private; notify everyone currently in the room.</summary>
public sealed record MatchRoomBecamePrivateNotifyParticipantsDomainEvent(
    Guid RoomId,
    string? RoomName,
    IReadOnlyList<Guid> ParticipantUserIds) : IDomainEvent;
