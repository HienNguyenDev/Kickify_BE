using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Infrastructure.Database;
using Kickify.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kickify.Infrastructure.Repositories;

public class VenueOperatingHourRepository : GenericRepository<VenueOperatingHour>, IVenueOperatingHourRepository
{
    public VenueOperatingHourRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<List<VenueOperatingHour>> GetByVenueIdAsync(Guid venueId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(oh => oh.VenueId == venueId)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<VenueOperatingHour>> GetByVenueIdOrderedAsync(Guid venueId, CancellationToken cancellationToken = default)
    {
        var hours = await _dbSet
            .AsNoTracking()
            .Where(oh => oh.VenueId == venueId)
            .ToListAsync(cancellationToken);

        return hours.OrderBy(h => (int)h.DayOfWeek).ToList();
    }

    public async Task<VenueOperatingHour?> GetByVenueAndDayAsync(Guid venueId, DayOfWeekEnum dayOfWeek, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(oh => oh.VenueId == venueId && oh.DayOfWeek == dayOfWeek, cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<VenueOperatingHour> operatingHours, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddRangeAsync(operatingHours, cancellationToken);
    }
}
