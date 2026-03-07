using Kickify.Domain.Common;

namespace Kickify.Domain.Errors;

public static class NotificationErrors
{
    public static readonly Error PreferenceNotFound = Error.NotFound(
        "NotificationPreference.NotFound",
        "Notification preferences not found for current user");
}
