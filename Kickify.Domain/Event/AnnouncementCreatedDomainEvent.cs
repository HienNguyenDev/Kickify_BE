using Kickify.Domain.Common;
using Kickify.Domain.Enums;

namespace Kickify.Domain.Event;

public record AnnouncementCreatedDomainEvent(
    Guid AnnouncementId,
    Guid CreatedBy,
    string Title,
    string Content,
    AnnouncementType AnnouncementType) : IDomainEvent;
