using Kickify.Application.Abstractions.Persistence;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;

namespace Kickify.Application.Abstractions.Repositories
{
    public interface IMatchFormationRepository : IGenericRepository<MatchFormation>
    {
        Task<MatchFormation?> GetFormationByRoomAndTeamAsync(Guid roomId, TeamAssignment team, CancellationToken cancellationToken = default);
        Task<MatchFormation?> GetFormationWithAssignmentsAsync(Guid formationId, CancellationToken cancellationToken = default);
        Task<List<MatchFormation>> GetFormationsByRoomAsync(Guid roomId, CancellationToken cancellationToken = default);
    }
}
