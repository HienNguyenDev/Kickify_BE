using Kickify.Domain.Common;

namespace Kickify.Domain.Errors;

public static class PremiumErrors
{
    public static readonly Error PremiumRequired = Error.Conflict(
        "Premium.Required",
        "This feature requires an active Kickify Premium subscription. Purchase Premium at /api/premium/purchase.");

    public static readonly Error PremiumExpired = Error.Conflict(
        "Premium.Expired",
        "Your Premium subscription has expired. Renew at /api/premium/purchase.");
}
