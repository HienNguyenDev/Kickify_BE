using Kickify.Application.Abstractions.Persistence;
using Kickify.Domain.Entities;

namespace Kickify.Application.Abstractions.Repositories;

public interface IVenueEvidenceRepository : IGenericRepository<VenueEvidence>
{
    Task<List<VenueEvidence>> GetByVenueIdAsync(Guid venueId, CancellationToken cancellationToken = default);
    Task<VenueEvidence?> GetByIdWithVenueAsync(Guid evidenceId, CancellationToken cancellationToken = default);
    Task<int> CountByVenueIdAsync(Guid venueId, CancellationToken cancellationToken = default);
}
