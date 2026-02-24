using Kickify.Application.Abstractions.Persistence;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;

namespace Kickify.Application.Abstractions.Repositories
{
    public interface IVenueRepository : IGenericRepository<Venue>
    {
        Task<Venue?> GetVenueWithDetailsAsync(Guid venueId, CancellationToken cancellationToken = default);
        Task<bool> IsOwnerAsync(Guid venueId, Guid userId, CancellationToken cancellationToken = default);
        Task<(IEnumerable<Venue> Venues, int Total)> SearchVenuesAsync(
            decimal? latitude = null,
            decimal? longitude = null,
            double? radiusKm = null,
            DateTime? date = null,
            FieldType? fieldType = null,
            string? searchName = null,
            VenueStatus? status = null,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get venue for update (WITH tracking)
        /// </summary>
        Task<Venue?> GetVenueForUpdateAsync(
            Guid venueId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get paged venues by owner
        /// </summary>
        Task<(IEnumerable<Venue> Venues, int Total)> GetVenuesByOwnerPagedAsync(
            Guid ownerId,
            string? searchName = null,
            VenueStatus? status = null,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get max display order for venue photos
        /// </summary>
        Task<int> GetMaxPhotoDisplayOrderAsync(
            Guid venueId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Add photos to venue
        /// </summary>
        Task AddVenuePhotosAsync(
            IEnumerable<VenuePhoto> photos,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get total booking counts for multiple venues (via Fields → Bookings)
        /// </summary>
        Task<Dictionary<Guid, int>> GetBookingCountsByVenueIdsAsync(
            IEnumerable<Guid> venueIds,
            CancellationToken cancellationToken = default);
    }
}
