using Kickify.Domain.Common;

namespace Kickify.Domain.Errors;

public static class PlayerReportErrors
{
    public static Error NotFound(Guid reportId) => Error.NotFound(
        "Report.NotFound",
        $"Report with ID '{reportId}' not found");

    public static readonly Error SelfReport = Error.Conflict(
        "Report.SelfReport",
        "You cannot report yourself");

    public static readonly Error AlreadyReported = Error.Conflict(
        "Report.AlreadyReported",
        "You already have a pending report against this user");

    public static readonly Error AlreadyProcessed = Error.Conflict(
        "Report.AlreadyProcessed",
        "This report has already been processed");
}
