using Kickify.Application.Abstractions.Messaging;
using Kickify.Domain.Common;
using Kickify.Domain.Enums;

namespace Kickify.Application.Features.ContentReports.Commands.ReportContent;

public class ReportContentCommand : ICommand<ReportContentResponse>
{
    public ContentType ContentType { get; set; }
    public Guid ContentId { get; set; }
    public ContentReportReason Reason { get; set; }
    public string? Description { get; set; }
}
