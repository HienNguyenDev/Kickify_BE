using Kickify.Domain.Enums;

namespace Kickify.Api.Requests;

public class ReportContentRequest
{
    public ContentReportReason Reason { get; set; }
    public string Description { get; set; } = string.Empty;
}
