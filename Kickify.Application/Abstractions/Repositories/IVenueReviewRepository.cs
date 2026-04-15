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

        /// <summary>
        /// Get a review by ID with navigation properties (Venue, User, Booking)
        /// </summary>
        Task<VenueReview?> GetByIdWithDetailsAsync(Guid reviewId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get a review by ID with venue for update (tracked). Used for owner reply.
        /// </summary>
        Task<VenueReview?> GetByIdWithVenueForUpdateAsync(Guid reviewId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get all reviews with filters and pagination, including navigation properties
        /// </summary>
        Task<(IEnumerable<VenueReview> Items, int Total)> GetAllPagedAsync(
            Guid? venueId = null,
            Guid? userId = null,
            int? minRating = null,
            int? maxRating = null,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get reviews for venues owned by a specific owner, with filters and pagination
        /// </summary>
        Task<(IEnumerable<VenueReview> Items, int Total)> GetByVenueOwnerPagedAsync(
            Guid ownerId,
            Guid? venueId = null,
            int? minRating = null,
            int? maxRating = null,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default);
    }
}
