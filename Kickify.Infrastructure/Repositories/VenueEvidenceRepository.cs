using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Entities;
using Kickify.Infrastructure.Database;
using Kickify.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kickify.Infrastructure.Repositories;

public class VenueEvidenceRepository : GenericRepository<VenueEvidence>, IVenueEvidenceRepository
{
    public VenueEvidenceRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<List<VenueEvidence>> GetByVenueIdAsync(Guid venueId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(e => e.VenueId == venueId)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<VenueEvidence?> GetByIdWithVenueAsync(Guid evidenceId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(e => e.Venue)
            .FirstOrDefaultAsync(e => e.EvidenceId == evidenceId, cancellationToken);
    }

    public async Task<int> CountByVenueIdAsync(Guid venueId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.CountAsync(e => e.VenueId == venueId, cancellationToken);
    }
}
