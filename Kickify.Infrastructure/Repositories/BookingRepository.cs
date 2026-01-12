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
    }
}
