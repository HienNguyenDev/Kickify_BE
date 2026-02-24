using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Infrastructure.Database;
using Kickify.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kickify.Infrastructure.Repositories
{
    public class MatchFormationRepository : GenericRepository<MatchFormation>, IMatchFormationRepository
    {
        public MatchFormationRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<MatchFormation?> GetFormationByRoomAndTeamAsync(Guid roomId, TeamAssignment team, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(f => f.Assignments)
                    .ThenInclude(a => a.Player)
                .FirstOrDefaultAsync(f => f.RoomId == roomId && f.TeamAssignment == team, cancellationToken);
        }

        public async Task<MatchFormation?> GetFormationWithAssignmentsAsync(Guid formationId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(f => f.Assignments)
                    .ThenInclude(a => a.Player)
                .FirstOrDefaultAsync(f => f.FormationId == formationId, cancellationToken);
        }

        public async Task<List<MatchFormation>> GetFormationsByRoomAsync(Guid roomId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(f => f.Assignments)
                    .ThenInclude(a => a.Player)
                .Where(f => f.RoomId == roomId)
                .ToListAsync(cancellationToken);
        }
    }
}
