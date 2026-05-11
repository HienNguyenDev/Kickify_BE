using Kickify.Domain.Common;

namespace Kickify.Domain.Errors;

public static class BanErrors
{
    public static readonly Error CannotBanAdmin = Error.Conflict(
        "Ban.CannotBanAdmin",
        "Cannot ban an admin account");
    
    public static readonly Error AlreadyBanned = Error.Conflict(
        "Ban.AlreadyBanned",
        "The user is already banned");
    
    public static readonly Error NotBanned = Error.Conflict(
        "Ban.NotBanned",
        "The user is not currently banned");
}
