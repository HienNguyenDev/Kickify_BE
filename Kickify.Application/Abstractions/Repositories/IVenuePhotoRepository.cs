using Kickify.Application.Abstractions.Persistence;
using Kickify.Domain.Entities;

namespace Kickify.Application.Abstractions.Repositories
{
    public interface IVenuePhotoRepository : IGenericRepository<VenuePhoto>
    {
        /// <summary>
        /// Get all photos of a venue ordered by display order
        /// </summary>
        Task<IEnumerable<VenuePhoto>> GetPhotosByVenueIdAsync(
            Guid venueId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get all photos for multiple venues in a single query
        /// </summary>
        Task<Dictionary<Guid, List<VenuePhoto>>> GetPhotosByVenueIdsAsync(
            IEnumerable<Guid> venueIds,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get photo by ID with tracking for update
        /// </summary>
        Task<VenuePhoto?> GetPhotoForUpdateAsync(
            Guid photoId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get photo by ID with venue info (for ownership check)
        /// </summary>
        Task<VenuePhoto?> GetPhotoWithVenueAsync(
            Guid photoId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get max display order for venue photos
        /// </summary>
        Task<int> GetMaxDisplayOrderAsync(
            Guid venueId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Add multiple photos
        /// </summary>
        Task AddPhotosAsync(
            IEnumerable<VenuePhoto> photos,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Check if venue exists
        /// </summary>
        Task<bool> VenueExistsAsync(
            Guid venueId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Check if user is owner of venue
        /// </summary>
        Task<bool> IsVenueOwnerAsync(
            Guid venueId,
            Guid userId,
            CancellationToken cancellationToken = default);
    }
}
