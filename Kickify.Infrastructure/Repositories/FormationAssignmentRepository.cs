using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Entities;
using Kickify.Infrastructure.Database;
using Kickify.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kickify.Infrastructure.Repositories
{
    public class FormationAssignmentRepository : GenericRepository<FormationAssignment>, IFormationAssignmentRepository
    {
        public FormationAssignmentRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<FormationAssignment>> GetAssignmentsByFormationAsync(Guid formationId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(a => a.Player)
                .Where(a => a.FormationId == formationId)
                .ToListAsync(cancellationToken);
        }

        public async Task DeleteByFormationIdAsync(Guid formationId, CancellationToken cancellationToken = default)
        {
            var assignments = await _dbSet
                .Where(a => a.FormationId == formationId)
                .ToListAsync(cancellationToken);

            if (assignments.Any())
            {
                _dbSet.RemoveRange(assignments);
            }
        }
    }
}
