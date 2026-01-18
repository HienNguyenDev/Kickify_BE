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
    }
}
