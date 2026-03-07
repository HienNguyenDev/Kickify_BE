using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;

namespace Kickify.Application.Features.PlayerReports.Queries.GetReports;

public class GetReportsQueryHandler : IQueryHandler<GetReportsQuery, GetReportsResponse>
{
    private readonly IPlayerReportRepository _reportRepository;

    public GetReportsQueryHandler(IPlayerReportRepository reportRepository)
    {
        _reportRepository = reportRepository;
    }

    public async Task<Result<GetReportsResponse>> Handle(GetReportsQuery request, CancellationToken cancellationToken)
    {
        var (reports, total) = await _reportRepository.GetPagedReportsAsync(
            request.Status,
            request.ReportedUserId,
            request.Page,
            request.PageSize,
            cancellationToken);

        var dtos = reports.Select(r => new ReportDto(
            r.ReportId,
            r.ReporterId,
            r.Reporter.FullName ?? r.Reporter.Email,
            r.Reporter.Email,
            r.ReportedId,
            r.Reported.FullName ?? r.Reported.Email,
            r.Reported.Email,
            r.ReportType.ToString(),
            r.Description,
            r.Status.ToString(),
            r.AdminNotes,
            r.ActionTaken,
            r.ResolvedBy,
            r.ResolvedAt,
            r.CreatedAt)).ToList();

        var totalPages = (int)Math.Ceiling((double)total / request.PageSize);

        return Result.Success(new GetReportsResponse(dtos, total, request.Page, request.PageSize, totalPages));
    }
}
