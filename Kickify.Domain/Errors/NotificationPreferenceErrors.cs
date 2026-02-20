using Kickify.Domain.Common;

namespace Kickify.Domain.Errors;

public static class NotificationPreferenceErrors
{
    public static readonly Error NotFound = Error.NotFound("NotificationPreferences.NotFound", "Notification preference not found for this user");
}
