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
    }
}
