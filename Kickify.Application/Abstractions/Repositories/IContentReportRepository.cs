using Kickify.Application.Abstractions.Persistence;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;

namespace Kickify.Application.Abstractions.Repositories;

public interface IContentReportRepository : IGenericRepository<ContentReport>
{
    Task<(List<ContentReport> Reports, int Total)> GetPagedReportsAsync(
        ReportStatus? status = null,
        ContentType? contentType = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);

    Task<ContentReport?> GetByIdWithDetailsAsync(Guid reportId, CancellationToken cancellationToken = default);

    Task<bool> HasAlreadyReportedAsync(Guid reporterId, Guid contentId, CancellationToken cancellationToken = default);
}
