using Kickify.Application.Abstractions.Persistence;
using Kickify.Domain.Entities;

namespace Kickify.Application.Abstractions.Repositories
{
    public interface IFormationAssignmentRepository : IGenericRepository<FormationAssignment>
    {
        Task<List<FormationAssignment>> GetAssignmentsByFormationAsync(Guid formationId, CancellationToken cancellationToken = default);
        Task DeleteByFormationIdAsync(Guid formationId, CancellationToken cancellationToken = default);
    }
}
