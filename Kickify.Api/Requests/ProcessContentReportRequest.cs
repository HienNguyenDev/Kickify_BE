using Kickify.Domain.Enums;

namespace Kickify.Api.Requests;

public class ProcessContentReportRequest
{
    public ReportStatus Action { get; set; }
    public string? AdminNotes { get; set; }
}
