using Kickify.Domain.Common;

namespace Kickify.Domain.Event;

public sealed record FriendRequestSentDomainEvent : IDomainEvent
{
    public Guid FriendshipId { get; }
    public Guid RequesterId { get; }
    public Guid AddresseeId { get; }
    public string RequesterName { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public FriendRequestSentDomainEvent(Guid friendshipId, Guid requesterId, Guid addresseeId, string requesterName)
    {
        FriendshipId = friendshipId;
        RequesterId = requesterId;
        AddresseeId = addresseeId;
        RequesterName = requesterName;
    }
}
