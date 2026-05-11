using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Infrastructure.Database;
using Kickify.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kickify.Infrastructure.Repositories;

public class PlayerReportRepository : GenericRepository<PlayerReport>, IPlayerReportRepository
{
    public PlayerReportRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<(List<PlayerReport> Reports, int Total)> GetPagedReportsAsync(
        ReportStatus? status = null,
        Guid? reportedUserId = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .AsNoTracking()
            .Include(r => r.Reporter)
            .Include(r => r.Reported)
            .Include(r => r.Resolver)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(r => r.Status == status.Value);

        if (reportedUserId.HasValue)
            query = query.Where(r => r.ReportedId == reportedUserId.Value);

        var total = await query.CountAsync(cancellationToken);

        var reports = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (reports, total);
    }

    public async Task<PlayerReport?> GetByIdWithDetailsAsync(Guid reportId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(r => r.Reporter)
            .Include(r => r.Reported)
            .Include(r => r.Resolver)
            .FirstOrDefaultAsync(r => r.ReportId == reportId, cancellationToken);
    }

    public async Task<bool> HasPendingReportAsync(Guid reporterId, Guid reportedId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AnyAsync(r => r.ReporterId == reporterId
                        && r.ReportedId == reportedId
                        && r.Status == ReportStatus.Pending,
                cancellationToken);
    }
}
