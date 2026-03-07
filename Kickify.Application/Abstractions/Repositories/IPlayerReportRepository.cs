using Kickify.Application.Abstractions.Persistence;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;

namespace Kickify.Application.Abstractions.Repositories;

public interface IPlayerReportRepository : IGenericRepository<PlayerReport>
{
    Task<(List<PlayerReport> Reports, int Total)> GetPagedReportsAsync(
        ReportStatus? status = null,
        Guid? reportedUserId = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);

    Task<PlayerReport?> GetByIdWithDetailsAsync(Guid reportId, CancellationToken cancellationToken = default);

    Task<bool> HasPendingReportAsync(Guid reporterId, Guid reportedId, CancellationToken cancellationToken = default);
}
