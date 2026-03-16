using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
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
                    .ThenInclude(v => v.IgnoredHolidays)
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

        public async Task<Field?> GetFieldWithVenueForUpdateAsync(
            Guid fieldId,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(f => f.Venue)
                .FirstOrDefaultAsync(f => f.FieldId == fieldId, cancellationToken);
        }

        public async Task<(IEnumerable<Field> Fields, int Total)> GetFieldsPagedAsync(
            FieldType? fieldType = null,
            bool? isActive = null,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var query = _dbSet
                .AsNoTracking()
                .Include(f => f.Venue)
                .AsQueryable();

            if (fieldType.HasValue)
            {
                query = query.Where(f => f.FieldType == fieldType.Value);
            }

            if (isActive.HasValue)
            {
                query = query.Where(f => f.IsActive == isActive.Value);
            }

            var total = await query.CountAsync(cancellationToken);

            var fields = await query
                .OrderByDescending(f => f.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (fields, total);
        }

        public async Task<(IEnumerable<Field> Fields, int Total)> GetFieldsByOwnerPagedAsync(
            Guid ownerId,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var query = _dbSet
                .AsNoTracking()
                .Include(f => f.Venue)
                .Where(f => f.Venue.OwnerId == ownerId);

            var total = await query.CountAsync(cancellationToken);

            var fields = await query
                .OrderByDescending(f => f.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (fields, total);
        }
    }
}
