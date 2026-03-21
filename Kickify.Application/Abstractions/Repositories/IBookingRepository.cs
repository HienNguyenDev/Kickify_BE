using Kickify.Application.Abstractions.Persistence;
using Kickify.Domain.Entities;

namespace Kickify.Application.Abstractions.Repositories
{
    public interface IBookingRepository : IGenericRepository<Booking>
    {
        Task<IEnumerable<Booking>> GetBookingsByFieldAndDateAsync(
            Guid fieldId, 
            DateTime date, 
            CancellationToken cancellationToken = default);
        Task<Booking?> GetBookingByRoomAsync(Guid roomId, CancellationToken cancellationToken = default);
        Task<IEnumerable<(TimeSpan StartTime, TimeSpan EndTime)>> GetBookedTimeSlotsAsync(
            Guid fieldId,
            DateTime date,
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Check if a time slot overlaps with any existing bookings
        /// </summary>
        Task<bool> IsTimeSlotAvailableAsync(
            Guid fieldId,
            DateTime date,
            TimeSpan startTime,
            TimeSpan endTime,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get booking with full details (field, venue)
        /// </summary>
        Task<Booking?> GetBookingWithDetailsAsync(
            Guid bookingId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get booking with field, venue, match room, and room participants for review validation
        /// </summary>
        Task<Booking?> GetBookingForReviewValidationAsync(
            Guid bookingId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get paged bookings with optional filters
        /// </summary>
        Task<(IEnumerable<Booking> Bookings, int Total)> GetBookingsPagedAsync(
            Guid? fieldId = null,
            DateTime? date = null,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get paged bookings for all venues owned by a specific venue owner
        /// </summary>
        Task<(IEnumerable<Booking> Bookings, int Total)> GetBookingsByVenueOwnerPagedAsync(
            Guid ownerId,
            Guid? fieldId = null,
            DateTime? date = null,
            string? status = null,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default);

        Task<Booking?> GetEligibleBookingForVenueReviewAsync(Guid venueId, Guid userId, CancellationToken cancellationToken = default);
    }
}
