namespace Kickify.Application.Features.ContentReports.Commands.ReportContent;

public class ReportContentResponse
{
    public Guid ReportId { get; set; }
    public string Message { get; set; } = string.Empty;
}
