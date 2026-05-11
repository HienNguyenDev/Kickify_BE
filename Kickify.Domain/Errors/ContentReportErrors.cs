using Kickify.Domain.Common;
using Kickify.Domain.Enums;

namespace Kickify.Domain.Errors;

public static class ContentReportErrors
{
    public static Error NotFound(Guid reportId) =>
        Error.NotFound("ContentReport.NotFound", $"Content report with ID '{reportId}' was not found.");

    public static readonly Error SelfReport =
        Error.Conflict("ContentReport.SelfReport", "You cannot report your own content.");

    public static readonly Error AlreadyReported =
        Error.Conflict("ContentReport.AlreadyReported", "You have already reported this content.");

    public static readonly Error AlreadyProcessed =
        Error.Conflict("ContentReport.AlreadyProcessed", "This report has already been processed.");

    public static Error ContentNotFound(ContentType contentType, Guid contentId) =>
        Error.NotFound("ContentReport.ContentNotFound", $"{contentType} with ID '{contentId}' was not found.");
}
