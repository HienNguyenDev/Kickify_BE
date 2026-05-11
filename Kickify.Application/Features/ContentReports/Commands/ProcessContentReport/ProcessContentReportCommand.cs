using Kickify.Application.Abstractions.Messaging;
using Kickify.Domain.Common;
using Kickify.Domain.Enums;

namespace Kickify.Application.Features.ContentReports.Commands.ProcessContentReport;

public class ProcessContentReportCommand : ICommand<ProcessContentReportResponse>
{
    public Guid ReportId { get; set; }
    public ReportStatus Action { get; set; } // Resolved or Dismissed
    public string? AdminNotes { get; set; }
}
