using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Entities;
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
    }
}
