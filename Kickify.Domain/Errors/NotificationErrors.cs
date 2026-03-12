using Kickify.Domain.Common;

namespace Kickify.Domain.Errors;

public static class NotificationErrors
{
    public static readonly Error PreferenceNotFound = Error.NotFound(
        "NotificationPreference.NotFound",
        "Notification preferences not found for current user");

    public static Error NotFound(Guid notificationId) => Error.NotFound(
        "Notification.NotFound",
        $"Notification with ID '{notificationId}' was not found");

    public static readonly Error Unauthorized = Error.Conflict(
        "Notification.Unauthorized",
        "You are not authorized to access this notification");
}
