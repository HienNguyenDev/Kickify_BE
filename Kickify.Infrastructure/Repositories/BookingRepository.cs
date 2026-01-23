using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Entities;
using Kickify.Infrastructure.Database;
using Kickify.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kickify.Infrastructure.Repositories
{
    public class BookingRepository : GenericRepository<Booking>, IBookingRepository
    {
        public BookingRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Booking>> GetBookingsByFieldAndDateAsync(
            Guid fieldId, 
            DateTime date, 
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(b => b.FieldId == fieldId && b.BookingDate == date)
                .OrderBy(b => b.StartTime)
                .ToListAsync(cancellationToken);
        }

        public async Task<Booking?> GetBookingByRoomAsync(Guid roomId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(b => b.Field)
                    .ThenInclude(f => f.Venue)
                .FirstOrDefaultAsync(b => b.RoomId == roomId, cancellationToken);
        }

        public async Task<IEnumerable<(TimeSpan StartTime, TimeSpan EndTime)>> GetBookedTimeSlotsAsync(
            Guid fieldId,
            DateTime date,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(b => b.FieldId == fieldId && b.BookingDate == date)
                .Select(b => new ValueTuple<TimeSpan, TimeSpan>(b.StartTime, b.EndTime))
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Check if a time slot overlaps with any existing bookings
        /// Returns true if available (no overlap), false if overlaps
        /// </summary>
        public async Task<bool> IsTimeSlotAvailableAsync(
            Guid fieldId,
            DateTime date,
            TimeSpan startTime,
            TimeSpan endTime,
            CancellationToken cancellationToken = default)
        {
            // Check for any overlapping bookings
            // Two time slots overlap if: startTime1 < endTime2 AND endTime1 > startTime2
            var hasOverlap = await _dbSet
                .AsNoTracking()
                .AnyAsync(b => 
                    b.FieldId == fieldId && 
                    b.BookingDate == date &&
                    b.StartTime < endTime &&
                    b.EndTime > startTime,
                    cancellationToken);

            return !hasOverlap; // Available if no overlap
        }

        public async Task<Booking?> GetBookingWithDetailsAsync(
            Guid bookingId,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(b => b.Field)
                    .ThenInclude(f => f.Venue)
                .Include(b => b.MatchRoom)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId, cancellationToken);
        }

        public async Task<(IEnumerable<Booking> Bookings, int Total)> GetBookingsPagedAsync(
            Guid? fieldId = null,
            DateTime? date = null,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var query = _dbSet
                .AsNoTracking()
                .Include(b => b.Field)
                    .ThenInclude(f => f.Venue)
                .AsQueryable();

            if (fieldId.HasValue)
            {
                query = query.Where(b => b.FieldId == fieldId.Value);
            }

            if (date.HasValue)
            {
                query = query.Where(b => b.BookingDate == date.Value);
            }

            var total = await query.CountAsync(cancellationToken);

            var bookings = await query
                .OrderByDescending(b => b.BookingDate)
                .ThenByDescending(b => b.StartTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (bookings, total);
        }
    }
}
