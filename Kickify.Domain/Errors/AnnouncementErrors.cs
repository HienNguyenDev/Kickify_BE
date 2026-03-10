using Kickify.Domain.Common;

namespace Kickify.Domain.Errors;

public static class AnnouncementErrors
{
    public static Error NotFound(Guid announcementId) =>
        Error.NotFound("Announcement.NotFound", $"Announcement with ID '{announcementId}' was not found.");
}
