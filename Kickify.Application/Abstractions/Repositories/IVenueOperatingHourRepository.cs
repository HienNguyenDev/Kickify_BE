using Kickify.Application.Abstractions.Persistence;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;

namespace Kickify.Application.Abstractions.Repositories;

public interface IVenueOperatingHourRepository : IGenericRepository<VenueOperatingHour>
{
    Task<List<VenueOperatingHour>> GetByVenueIdAsync(Guid venueId, CancellationToken cancellationToken = default);
    Task<List<VenueOperatingHour>> GetByVenueIdOrderedAsync(Guid venueId, CancellationToken cancellationToken = default);
    Task<VenueOperatingHour?> GetByVenueAndDayAsync(Guid venueId, DayOfWeekEnum dayOfWeek, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<VenueOperatingHour> operatingHours, CancellationToken cancellationToken = default);
}
