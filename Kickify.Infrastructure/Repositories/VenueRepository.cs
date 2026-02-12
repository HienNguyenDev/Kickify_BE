using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Infrastructure.Database;
using Kickify.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kickify.Infrastructure.Repositories
{
    public class VenueRepository : GenericRepository<Venue>, IVenueRepository
    {
        public VenueRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Venue?> GetVenueWithDetailsAsync(Guid venueId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(v => v.Fields)
                .Include(v => v.VenueOperatingHours)
                .Include(v => v.VenuePhotos.OrderByDescending(p => p.DisplayOrder).Take(5))
                .FirstOrDefaultAsync(v => v.VenueId == venueId, cancellationToken);
        }

        public async Task<bool> IsOwnerAsync(Guid venueId, Guid userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AnyAsync(v => v.VenueId == venueId && v.OwnerId == userId, cancellationToken);
        }

        public async Task<(IEnumerable<Venue> Venues, int Total)> SearchVenuesAsync(
            decimal? latitude = null,
            decimal? longitude = null,
            double? radiusKm = null,
            DateTime? date = null,
            FieldType? fieldType = null,
            string? searchName = null,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var query = _dbSet
                .AsNoTracking()
                .Include(v => v.Fields.Where(f => f.IsActive))
                .Include(v => v.VenuePhotos.OrderBy(p => p.DisplayOrder).Take(1))
                .AsQueryable();

            // Filter by venue name (case-insensitive search)
            if (!string.IsNullOrWhiteSpace(searchName))
            {
                var searchLower = searchName.ToLower();
                query = query.Where(v => v.VenueName.ToLower().Contains(searchLower));
            }

            // Filter by location (simplified - in production use PostGIS)
            if (latitude.HasValue && longitude.HasValue && radiusKm.HasValue)
            {
                // Simple bounding box calculation
                var latOffset = (decimal)(radiusKm.Value / 111.0); // ~111km per degree latitude
                var lonOffset = (decimal)(radiusKm.Value / (111.0 * Math.Cos(Convert.ToDouble(latitude.Value) * Math.PI / 180.0)));

                query = query.Where(v =>
                    v.Latitude >= latitude.Value - latOffset &&
                    v.Latitude <= latitude.Value + latOffset &&
                    v.Longitude >= longitude.Value - lonOffset &&
                    v.Longitude <= longitude.Value + lonOffset
                );
            }

            // Filter by field type (only venues with active fields of this type)
            if (fieldType.HasValue)
            {
                query = query.Where(v => v.Fields.Any(f => f.FieldType == fieldType.Value && f.IsActive));
            }

            // Filter by availability on specific date (if provided)
            if (date.HasValue)
            {
                var dayOfWeek = (DayOfWeekEnum)date.Value.DayOfWeek;
                query = query.Where(v => v.VenueOperatingHours.Any(oh => oh.DayOfWeek == dayOfWeek && !oh.IsClosed));
            }

            var total = await query.CountAsync(cancellationToken);

            var venues = await query
                .OrderByDescending(v => v.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            // If fieldType filter is applied, filter fields in-memory to only include matching fields
            if (fieldType.HasValue)
            {
                foreach (var venue in venues)
                {
                    var filteredFields = venue.Fields.Where(f => f.FieldType == fieldType.Value).ToList();
                    venue.Fields.Clear();
                    foreach (var field in filteredFields)
                    {
                        venue.Fields.Add(field);
                    }
                }
            }

            return (venues, total);
        }

        public async Task<Venue?> GetVenueForUpdateAsync(
            Guid venueId,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(v => v.VenueId == venueId, cancellationToken);
        }

        public async Task<(IEnumerable<Venue> Venues, int Total)> GetVenuesByOwnerPagedAsync(
            Guid ownerId,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var query = _dbSet
                .AsNoTracking()
                .Include(v => v.Fields)
                .Include(v => v.VenueOperatingHours)
                .Include(v => v.VenuePhotos)
                .Where(v => v.OwnerId == ownerId);

            var total = await query.CountAsync(cancellationToken);

            var venues = await query
                .OrderByDescending(v => v.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (venues, total);
        }

        public async Task<int> GetMaxPhotoDisplayOrderAsync(
            Guid venueId,
            CancellationToken cancellationToken = default)
        {
            return await _context.VenuePhotos
                .Where(vp => vp.VenueId == venueId)
                .MaxAsync(vp => (int?)vp.DisplayOrder, cancellationToken) ?? -1;
        }

        public async Task AddVenuePhotosAsync(
            IEnumerable<VenuePhoto> photos,
            CancellationToken cancellationToken = default)
        {
            await _context.VenuePhotos.AddRangeAsync(photos, cancellationToken);
        }
    }
}
