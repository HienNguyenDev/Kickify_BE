using Kickify.Domain.Common;

namespace Kickify.Domain.Errors;

public static class SystemLogErrors
{
    public static Error NotFound(Guid logId) => Error.NotFound(
        "SystemLogs.NotFound",
        $"System log with Id = '{logId}' was not found.");
}
