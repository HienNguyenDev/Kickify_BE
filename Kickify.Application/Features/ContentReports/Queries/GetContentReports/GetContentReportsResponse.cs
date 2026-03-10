using Kickify.Domain.Enums;

namespace Kickify.Application.Features.ContentReports.Queries.GetContentReports;

public class GetContentReportsResponse
{
    public List<ContentReportDto> Reports { get; set; } = [];
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

public class ContentReportDto
{
    public Guid ReportId { get; set; }
    public Guid ReporterId { get; set; }
    public string ReporterName { get; set; } = string.Empty;
    public ContentType ContentType { get; set; }
    public Guid ContentId { get; set; }
    public Guid ContentOwnerId { get; set; }
    public string ContentOwnerName { get; set; } = string.Empty;
    public ContentReportReason Reason { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? AdminNotes { get; set; }
    public string? ResolverName { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
