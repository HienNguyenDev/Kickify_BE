namespace Kickify.Application.Features.ContentReports.Commands.ProcessContentReport;

public class ProcessContentReportResponse
{
    public Guid ReportId { get; set; }
    public string Status { get; set; } = string.Empty;
}
