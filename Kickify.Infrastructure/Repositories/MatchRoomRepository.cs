using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Entities;
using Kickify.Infrastructure.Database;
using Kickify.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kickify.Infrastructure.Repositories
{
    public class MatchRoomRepository : GenericRepository<MatchRoom>, IMatchRoomRepository
    {
        public MatchRoomRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<MatchRoom?> GetRoomWithParticipantsAsync(Guid roomId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(r => r.RoomParticipants)
                .FirstOrDefaultAsync(r => r.RoomId == roomId, cancellationToken);
        }

        public async Task<MatchRoom?> GetRoomWithDetailsAsync(Guid roomId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(r => r.Host)
                .Include(r => r.Field)
                    .ThenInclude(f => f!.Venue)
                .Include(r => r.RoomParticipants)
                    .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(r => r.RoomId == roomId, cancellationToken);
        }

        public async Task<(IEnumerable<MatchRoom> Rooms, int Total)> SearchRoomsAsync(
            DateTime? date,
            string? matchFormat,
            bool? availableOnly,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var query = _dbSet
                .AsNoTracking()
                .Include(r => r.Host)
                .Include(r => r.Field)
                    .ThenInclude(f => f!.Venue)
                .AsQueryable();

            // Filter by date
            if (date.HasValue)
            {
                query = query.Where(r => r.MatchDate.Date == date.Value.Date);
            }

            // Filter by match format
            if (!string.IsNullOrEmpty(matchFormat))
            {
                if (Enum.TryParse<Domain.Enums.MatchFormat>(matchFormat, true, out var format))
                {
                    query = query.Where(r => r.MatchFormat == format);
                }
            }

            // Filter by availability
            if (availableOnly.HasValue && availableOnly.Value)
            {
                query = query.Where(r => r.FilledSlots < r.TotalSlots && r.Status == Domain.Enums.RoomStatus.Open);
            }

            var total = await query.CountAsync(cancellationToken);

            var rooms = await query
                .OrderBy(r => r.MatchDate)
                .ThenBy(r => r.StartTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (rooms, total);
        }

        public async Task<bool> AreAllParticipantsPaidAsync(Guid roomId, CancellationToken cancellationToken = default)
        {
            var room = await _dbSet
                .Include(r => r.RoomParticipants)
                .FirstOrDefaultAsync(r => r.RoomId == roomId, cancellationToken);

            if (room == null) return false;

            return room.RoomParticipants.All(p => p.DepositPaid);
        }

        public async Task<decimal> GetTotalPaidAmountAsync(Guid roomId, CancellationToken cancellationToken = default)
        {
            var room = await _dbSet
                .Include(r => r.RoomParticipants)
                .FirstOrDefaultAsync(r => r.RoomId == roomId, cancellationToken);

            if (room == null) return 0;

            return room.RoomParticipants.Where(p => p.DepositPaid).Sum(p => p.DepositAmount ?? 0);
        }
    }
}
