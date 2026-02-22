using Kickify.Application.Abstractions.Persistence;
using Kickify.Domain.Entities;

namespace Kickify.Application.Abstractions.Repositories
{
    public interface IVenueReviewRepository : IGenericRepository<VenueReview>
    {
        /// <summary>
        /// Check if a user has already reviewed a venue for a specific booking
        /// </summary>
        Task<bool> HasUserReviewedBookingAsync(Guid userId, Guid bookingId, CancellationToken cancellationToken = default);
    }
}
