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
