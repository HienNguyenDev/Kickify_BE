using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Entities;
using Kickify.Infrastructure.Database;
using Kickify.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kickify.Infrastructure.Repositories
{
    public class VenueReviewRepository : GenericRepository<VenueReview>, IVenueReviewRepository
    {
        public VenueReviewRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<bool> HasUserReviewedBookingAsync(Guid userId, Guid bookingId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .AnyAsync(r => r.UserId == userId && r.BookingId == bookingId, cancellationToken);
        }

        public async Task<VenueReview?> GetByIdWithDetailsAsync(Guid reviewId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(r => r.Venue)
                .Include(r => r.User)
                .Include(r => r.Booking)
                .FirstOrDefaultAsync(r => r.ReviewId == reviewId, cancellationToken);
        }

        public async Task<(IEnumerable<VenueReview> Items, int Total)> GetAllPagedAsync(
            Guid? venueId = null,
            Guid? userId = null,
            int? minRating = null,
            int? maxRating = null,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var query = _dbSet
                .AsNoTracking()
                .Include(r => r.Venue)
                .Include(r => r.User)
                .Include(r => r.Booking)
                .AsQueryable();

            if (venueId.HasValue)
                query = query.Where(r => r.VenueId == venueId.Value);

            if (userId.HasValue)
                query = query.Where(r => r.UserId == userId.Value);

            if (minRating.HasValue)
                query = query.Where(r => r.Rating >= minRating.Value);

            if (maxRating.HasValue)
                query = query.Where(r => r.Rating <= maxRating.Value);

            var total = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, total);
        }

        public async Task<(IEnumerable<VenueReview> Items, int Total)> GetByVenueOwnerPagedAsync(
            Guid ownerId,
            Guid? venueId = null,
            int? minRating = null,
            int? maxRating = null,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var query = _dbSet
                .AsNoTracking()
                .Include(r => r.Venue)
                .Include(r => r.User)
                .Include(r => r.Booking)
                .Where(r => r.Venue.OwnerId == ownerId);

            if (venueId.HasValue)
                query = query.Where(r => r.VenueId == venueId.Value);

            if (minRating.HasValue)
                query = query.Where(r => r.Rating >= minRating.Value);

            if (maxRating.HasValue)
                query = query.Where(r => r.Rating <= maxRating.Value);

            var total = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, total);
        }
    }
}
