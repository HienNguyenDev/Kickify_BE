using Kickify.Domain.Common;
using Kickify.Domain.Enums;

namespace Kickify.Domain.Event;

public record AnnouncementUpdatedDomainEvent(
    Guid AnnouncementId,
    Guid UpdatedBy,
    string Title,
    string Content,
    AnnouncementType AnnouncementType) : IDomainEvent;
