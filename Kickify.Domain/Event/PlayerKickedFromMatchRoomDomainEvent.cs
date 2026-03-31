using Kickify.Domain.Common;

namespace Kickify.Domain.Event;

/// <summary>Host removed a player; notify the kicked user.</summary>
public sealed record PlayerKickedFromMatchRoomDomainEvent(
    Guid RoomId,
    Guid KickedUserId,
    string? RoomName,
    string? Reason) : IDomainEvent;
