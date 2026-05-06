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

    public async Task<bool> ExistsByDateAsync(DateTime date, Guid? excludeHolidayId = null, CancellationToken cancellationToken = default)
    {
        var matchDate = date.Date;

        var query = _dbSet.AsNoTracking().Where(h => h.Date == matchDate);

        if (excludeHolidayId.HasValue)
        {
            query = query.Where(h => h.Id != excludeHolidayId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<List<DateTime>> GetExistingDatesAsync(IEnumerable<DateTime> dates, CancellationToken cancellationToken = default)
    {
        var matchDates = dates
            .Select(d => d.Date)
            .Distinct()
            .ToList();

        if (matchDates.Count == 0)
        {
            return new List<DateTime>();
        }

        return await _dbSet
            .AsNoTracking()
            .Where(h => matchDates.Contains(h.Date))
            .Select(h => h.Date)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<Holiday> holidays)
    {
        await _dbSet.AddRangeAsync(holidays);
    }

    public async Task<bool> HardDeleteByIdAsync(Guid holidayId, CancellationToken cancellationToken = default)
    {
        var affectedRows = await _dbSet
            .IgnoreQueryFilters()
            .Where(h => h.Id == holidayId)
            .ExecuteDeleteAsync(cancellationToken);

        return affectedRows > 0;
    }
}