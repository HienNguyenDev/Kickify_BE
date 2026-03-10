using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;

namespace Kickify.Application.Features.ContentReports.Queries.GetContentReports;

internal sealed class GetContentReportsQueryHandler : IQueryHandler<GetContentReportsQuery, GetContentReportsResponse>
{
    private readonly IContentReportRepository _contentReportRepository;

    public GetContentReportsQueryHandler(IContentReportRepository contentReportRepository)
    {
        _contentReportRepository = contentReportRepository;
    }

    public async Task<Result<GetContentReportsResponse>> Handle(GetContentReportsQuery request, CancellationToken cancellationToken)
    {
        var (reports, total) = await _contentReportRepository.GetPagedReportsAsync(
            status: request.Status,
            contentType: request.ContentType,
            page: request.Page,
            pageSize: request.PageSize,
            cancellationToken: cancellationToken);

        var dtos = reports.Select(r => new ContentReportDto
        {
            ReportId = r.ReportId,
            ReporterId = r.ReporterId,
            ReporterName = r.Reporter.FullName,
            ContentType = r.ContentType,
            ContentId = r.ContentId,
            ContentOwnerId = r.ContentOwnerId,
            ContentOwnerName = r.ContentOwner.FullName,
            Reason = r.Reason,
            Description = r.Description,
            Status = r.Status.ToString(),
            AdminNotes = r.AdminNotes,
            ResolverName = r.Resolver?.FullName,
            ResolvedAt = r.ResolvedAt,
            CreatedAt = r.CreatedAt
        }).ToList();

        return Result.Success(new GetContentReportsResponse
        {
            Reports = dtos,
            Total = total,
            Page = request.Page,
            PageSize = request.PageSize
        });
    }
}
