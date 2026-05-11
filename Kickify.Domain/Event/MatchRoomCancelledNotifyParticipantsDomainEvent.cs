using Kickify.Domain.Common;

namespace Kickify.Domain.Event;

/// <summary>Room was cancelled (e.g. auto-close); notify all members.</summary>
public sealed record MatchRoomCancelledNotifyParticipantsDomainEvent(
    Guid RoomId,
    string? RoomName,
    IReadOnlyList<Guid> ParticipantUserIds) : IDomainEvent;
