using Kickify.Application.Abstractions.Messaging;
using Kickify.Domain.Common;
using Kickify.Domain.Enums;

namespace Kickify.Application.Features.ContentReports.Queries.GetContentReports;

public class GetContentReportsQuery : IQuery<GetContentReportsResponse>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public ReportStatus? Status { get; set; }
    public ContentType? ContentType { get; set; }
}
