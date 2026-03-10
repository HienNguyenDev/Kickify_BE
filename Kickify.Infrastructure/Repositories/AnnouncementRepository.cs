using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Infrastructure.Database;
using Kickify.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kickify.Infrastructure.Repositories;

public class AnnouncementRepository : GenericRepository<Announcement>, IAnnouncementRepository
{
    public AnnouncementRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<(List<Announcement> Announcements, int Total)> GetPagedAsync(
        AnnouncementType? announcementType = null,
        bool? isActive = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsNoTracking().AsQueryable();

        if (announcementType.HasValue)
            query = query.Where(a => a.AnnouncementType == announcementType.Value);

        if (isActive.HasValue)
            query = query.Where(a => a.IsActive == isActive.Value);

        var total = await query.CountAsync(cancellationToken);

        var announcements = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (announcements, total);
    }

    public async Task<Announcement?> GetByIdAsync(Guid announcementId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.AnnouncementId == announcementId, cancellationToken);
    }
}
