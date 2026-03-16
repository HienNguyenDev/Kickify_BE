using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Entities;
using Kickify.Infrastructure.Database;
using Kickify.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kickify.Infrastructure.Repositories;

public class HolidayRepository : GenericRepository<Holiday>, IHolidayRepository
{
    public HolidayRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Holiday>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .OrderBy(h => h.Date)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Holiday>> GetByIdsAsync(IEnumerable<Guid> holidayIds, CancellationToken cancellationToken = default)
    {
        var ids = holidayIds.Distinct().ToList();

        if (ids.Count == 0)
        {
            return Array.Empty<Holiday>();
        }

        return await _dbSet
            .Where(h => ids.Contains(h.Id))
            .OrderBy(h => h.Date)
            .ToListAsync(cancellationToken);
    }

    public async Task<Holiday?> GetByDateAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        var matchDate = date.Date;

        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(h => h.Date == matchDate, cancellationToken);
    }
}