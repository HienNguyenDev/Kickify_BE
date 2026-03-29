using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
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

        /// <summary>
        /// Get room with participants WITH TRACKING for update/delete operations
        /// </summary>
        public async Task<MatchRoom?> GetRoomWithParticipantsForUpdateAsync(Guid roomId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
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
            decimal? latitude,
            decimal? longitude,
            double? radiusKm,
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

            // Filter by location (bounding box)
            if (latitude.HasValue && longitude.HasValue && radiusKm.HasValue)
            {
                var latOffset = (decimal)(radiusKm.Value / 111.0);
                var lonOffset = (decimal)(radiusKm.Value / (111.0 * Math.Cos(Convert.ToDouble(latitude.Value) * Math.PI / 180.0)));

                query = query.Where(r =>
                    r.Field != null && r.Field.Venue != null &&
                    r.Field.Venue.Latitude >= latitude.Value - latOffset &&
                    r.Field.Venue.Latitude <= latitude.Value + latOffset &&
                    r.Field.Venue.Longitude >= longitude.Value - lonOffset &&
                    r.Field.Venue.Longitude <= longitude.Value + lonOffset);
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

            // Check if room is full (FilledSlots == TotalSlots) AND all participants have paid
            var isRoomFull = room.FilledSlots >= room.TotalSlots;
            var allParticipantsPaid = room.RoomParticipants.All(p => p.DepositPaid);

            return isRoomFull && allParticipantsPaid;
        }

        public async Task<decimal> GetTotalPaidAmountAsync(Guid roomId, CancellationToken cancellationToken = default)
        {
            var room = await _dbSet
                .Include(r => r.RoomParticipants)
                .FirstOrDefaultAsync(r => r.RoomId == roomId, cancellationToken);

            if (room == null) return 0;

            return room.RoomParticipants.Where(p => p.DepositPaid).Sum(p => p.DepositAmount ?? 0);
        }

        public async Task<(IEnumerable<MatchRoom> Rooms, int Total)> GetMatchHistoryByUserAsync(
            Guid userId,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var query = _dbSet
                .AsNoTracking()
                .Include(r => r.Host)
                .Include(r => r.Field)
                    .ThenInclude(f => f!.Venue)
                .Include(r => r.RoomParticipants)
                .Where(r => r.RoomParticipants.Any(p => p.UserId == userId))
                .Where(r => r.Status == Kickify.Domain.Enums.RoomStatus.Reviewing || r.Status == Kickify.Domain.Enums.RoomStatus.Completed);
                //.Where(r => r.Visibility == Kickify.Domain.Enums.Visibility.Public);

            var total = await query.CountAsync(cancellationToken);

            var rooms = await query
                .OrderByDescending(r => r.MatchDate)
                .ThenByDescending(r => r.StartTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (rooms, total);
        }

        public async Task<(IEnumerable<MatchRoom> Rooms, int Total)> GetRoomsByUserAsync(
            Guid userId,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            // Get all rooms where user is a participant (including as host)
            var query = _dbSet
                .AsNoTracking()
                .Include(r => r.Host)
                .Include(r => r.Field)
                    .ThenInclude(f => f!.Venue)
                .Include(r => r.RoomParticipants)
                .Where(r => r.RoomParticipants.Any(p => p.UserId == userId));

            var total = await query.CountAsync(cancellationToken);

            var rooms = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (rooms, total);
        }

        public async Task<List<MatchRoom>> GetActiveRoomsForUserByDateAsync(Guid userId, DateTime matchDate, CancellationToken cancellationToken)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(r => r.MatchDate == matchDate
                         && (r.Status == RoomStatus.Open || r.Status == RoomStatus.Locked || r.Status == RoomStatus.InProgress)
                         && r.RoomParticipants.Any(p => p.UserId == userId))
                .ToListAsync(cancellationToken);
        }
    }
}
