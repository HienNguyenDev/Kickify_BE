using Kickify.Domain.Common;

namespace Kickify.Domain.Event;

public sealed record PrivateChatMessageSentDomainEvent(
    Guid MessageId,
    Guid ReceiverId,
    Guid SenderId,
    string SenderDisplayName,
    string MessagePreview) : IDomainEvent;
