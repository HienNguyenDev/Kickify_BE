using Kickify.Domain.Common;

namespace Kickify.Domain.Event;

/// <summary>Someone new joined; notify the room host (not sent when joiner is the host).</summary>
public sealed record MatchRoomPlayerJoinedHostNotifyDomainEvent(
    Guid RoomId,
    Guid HostUserId,
    string JoinerDisplayName,
    string? RoomName) : IDomainEvent;
