using Kickify.Domain.Common;

namespace Kickify.Domain.Event;

/// <summary>A participant left; notify the current host to find a replacement if needed.</summary>
public sealed record MatchRoomPlayerLeftHostNotifyDomainEvent(
    Guid RoomId,
    Guid HostUserId,
    string LeaverDisplayName,
    string? RoomName) : IDomainEvent;
