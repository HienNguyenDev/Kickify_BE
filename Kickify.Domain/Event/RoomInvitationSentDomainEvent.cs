using Kickify.Domain.Common;

namespace Kickify.Domain.Event;

public sealed record RoomInvitationSentDomainEvent : IDomainEvent
{
    public Guid InvitationId { get; }
    public Guid RoomId { get; }
    public Guid InviterId { get; }
    public Guid InviteeId { get; }
    public string InviterName { get; }
    public string? RoomName { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public RoomInvitationSentDomainEvent(
        Guid invitationId,
        Guid roomId,
        Guid inviterId,
        Guid inviteeId,
        string inviterName,
        string? roomName)
    {
        InvitationId = invitationId;
        RoomId = roomId;
        InviterId = inviterId;
        InviteeId = inviteeId;
        InviterName = inviterName;
        RoomName = roomName;
    }
}
