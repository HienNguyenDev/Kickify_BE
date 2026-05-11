using Kickify.Domain.Common;

namespace Kickify.Domain.Event;

public sealed record FriendRequestAcceptedDomainEvent : IDomainEvent
{
    public Guid FriendshipId { get; }
    public Guid RequesterId { get; }
    public Guid AddresseeId { get; }
    public string AddresseeName { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public FriendRequestAcceptedDomainEvent(Guid friendshipId, Guid requesterId, Guid addresseeId, string addresseeName)
    {
        FriendshipId = friendshipId;
        RequesterId = requesterId;
        AddresseeId = addresseeId;
        AddresseeName = addresseeName;
    }
}

