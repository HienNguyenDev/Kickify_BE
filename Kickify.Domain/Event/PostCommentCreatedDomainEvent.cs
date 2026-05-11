using Kickify.Domain.Common;

namespace Kickify.Domain.Event;

public sealed record PostCommentCreatedDomainEvent(
    Guid PostId,
    Guid CommentId,
    bool IsReply,
    Guid RecipientUserId,
    Guid ActorId,
    string ActorName) : IDomainEvent;
