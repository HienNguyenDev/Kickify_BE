using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Infrastructure.Database;
using Kickify.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kickify.Infrastructure.Repositories
{
    public class RoomParticipantRepository : GenericRepository<RoomParticipant>, IRoomParticipantRepository
    {
        public RoomParticipantRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<RoomParticipant?> GetParticipantByRoomAndUserAsync(Guid roomId, Guid userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(p => p.RoomId == roomId && p.UserId == userId, cancellationToken);
        }

        public async Task<bool> IsUserInRoomAsync(Guid roomId, Guid userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AnyAsync(p => p.RoomId == roomId && p.UserId == userId, cancellationToken);
        }

        public async Task<bool> HasTeamCaptainAsync(Guid roomId, TeamAssignment team, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AnyAsync(p => p.RoomId == roomId && p.TeamAssignment == team && p.IsCaptain, cancellationToken);
        }

        public async Task<Guid?> AssignNewCaptainAsync(Guid roomId, TeamAssignment team, Guid? excludeUserId = null, CancellationToken cancellationToken = default)
        {
            // Find the heir: earliest JoinDate in the same team (excluding the leaving user)
            var query = _dbSet
                .Where(p => p.RoomId == roomId && p.TeamAssignment == team);

            if (excludeUserId.HasValue)
            {
                query = query.Where(p => p.UserId != excludeUserId.Value);
            }

            var newCaptain = await query
                .OrderBy(p => p.JoinDate)
                .FirstOrDefaultAsync(cancellationToken);

            if (newCaptain == null)
            {
                return null; // No remaining players in the team
            }

            newCaptain.IsCaptain = true;
            return newCaptain.UserId;
        }
    }
}
