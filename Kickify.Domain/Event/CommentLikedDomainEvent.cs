using Kickify.Domain.Common;

namespace Kickify.Domain.Event;

public sealed record CommentLikedDomainEvent(
    Guid PostId,
    Guid CommentId,
    Guid RecipientUserId,
    Guid ActorId,
    string ActorName) : IDomainEvent;
