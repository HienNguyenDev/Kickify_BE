using Kickify.Domain.Common;

namespace Kickify.Domain.Event;

public sealed record PostLikedDomainEvent(
    Guid PostId,
    Guid RecipientUserId,
    Guid ActorId,
    string ActorName) : IDomainEvent;
