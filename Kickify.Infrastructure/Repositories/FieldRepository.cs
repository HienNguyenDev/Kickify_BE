using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Entities;
using Kickify.Infrastructure.Database;
using Kickify.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kickify.Infrastructure.Repositories
{
    public class FieldRepository : GenericRepository<Field>, IFieldRepository
    {
        public FieldRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Field?> GetFieldWithVenueAsync(Guid fieldId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(f => f.Venue)
                    .ThenInclude(v => v.VenueOperatingHours)
                .FirstOrDefaultAsync(f => f.FieldId == fieldId, cancellationToken);
        }

        public async Task<IEnumerable<Field>> GetFieldsByVenueAsync(Guid venueId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(f => f.VenueId == venueId)
                .OrderBy(f => f.FieldName)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> IsFieldAvailableAsync(
            Guid fieldId, 
            DateTime date, 
            TimeSpan startTime, 
            TimeSpan endTime, 
            CancellationToken cancellationToken = default)
        {
            // Check if there are any conflicting bookings
            var hasConflict = await _context.Set<Booking>()
                .AnyAsync(b =>
                    b.FieldId == fieldId &&
                    b.BookingDate == date &&
                    b.StartTime < endTime &&
                    b.EndTime > startTime,
                    cancellationToken);

            return !hasConflict;
        }
    }
}
