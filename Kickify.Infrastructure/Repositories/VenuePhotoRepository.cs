using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Entities;
using Kickify.Infrastructure.Database;
using Kickify.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kickify.Infrastructure.Repositories
{
    public class VenuePhotoRepository : GenericRepository<VenuePhoto>, IVenuePhotoRepository
    {
        public VenuePhotoRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<VenuePhoto>> GetPhotosByVenueIdAsync(
            Guid venueId,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(vp => vp.VenueId == venueId)
                .OrderBy(vp => vp.DisplayOrder)
                .ToListAsync(cancellationToken);
        }

        public async Task<Dictionary<Guid, List<VenuePhoto>>> GetPhotosByVenueIdsAsync(
            IEnumerable<Guid> venueIds,
            CancellationToken cancellationToken = default)
        {
            var photos = await _dbSet
                .AsNoTracking()
                .Where(vp => venueIds.Contains(vp.VenueId))
                .OrderBy(vp => vp.DisplayOrder)
                .ToListAsync(cancellationToken);

            return photos
                .GroupBy(vp => vp.VenueId)
                .ToDictionary(g => g.Key, g => g.ToList());
        }

        public async Task<VenuePhoto?> GetPhotoForUpdateAsync(
            Guid photoId,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(vp => vp.PhotoId == photoId, cancellationToken);
        }

        public async Task<VenuePhoto?> GetPhotoWithVenueAsync(
            Guid photoId,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(vp => vp.Venue)
                .FirstOrDefaultAsync(vp => vp.PhotoId == photoId, cancellationToken);
        }

        public async Task<int> GetMaxDisplayOrderAsync(
            Guid venueId,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(vp => vp.VenueId == venueId)
                .MaxAsync(vp => (int?)vp.DisplayOrder, cancellationToken) ?? -1;
        }

        public async Task AddPhotosAsync(
            IEnumerable<VenuePhoto> photos,
            CancellationToken cancellationToken = default)
        {
            await _dbSet.AddRangeAsync(photos, cancellationToken);
        }

        public async Task<bool> VenueExistsAsync(
            Guid venueId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Venues
                .AnyAsync(v => v.VenueId == venueId, cancellationToken);
        }

        public async Task<bool> IsVenueOwnerAsync(
            Guid venueId,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Venues
                .AnyAsync(v => v.VenueId == venueId && v.OwnerId == userId, cancellationToken);
        }
    }
}
