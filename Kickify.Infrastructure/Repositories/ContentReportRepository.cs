using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Infrastructure.Database;
using Kickify.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kickify.Infrastructure.Repositories;

public class ContentReportRepository : GenericRepository<ContentReport>, IContentReportRepository
{
    public ContentReportRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<(List<ContentReport> Reports, int Total)> GetPagedReportsAsync(
        ReportStatus? status = null,
        ContentType? contentType = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .AsNoTracking()
            .Include(r => r.Reporter)
            .Include(r => r.ContentOwner)
            .Include(r => r.Resolver)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(r => r.Status == status.Value);

        if (contentType.HasValue)
            query = query.Where(r => r.ContentType == contentType.Value);

        var total = await query.CountAsync(cancellationToken);

        var reports = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (reports, total);
    }

    public async Task<ContentReport?> GetByIdWithDetailsAsync(Guid reportId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(r => r.Reporter)
            .Include(r => r.ContentOwner)
            .Include(r => r.Resolver)
            .FirstOrDefaultAsync(r => r.ReportId == reportId, cancellationToken);
    }

    public async Task<bool> HasAlreadyReportedAsync(Guid reporterId, Guid contentId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AnyAsync(r => r.ReporterId == reporterId && r.ContentId == contentId, cancellationToken);
    }
}
