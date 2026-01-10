using Kickify.Application.Abstractions.Persistence;
using Kickify.Domain.Entities;

namespace Kickify.Application.Abstractions.Repositories
{
    public interface IPlayerProfileRepository : IGenericRepository<PlayerProfile>
    {
        Task<PlayerProfile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<bool> ExistsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<(IEnumerable<PlayerProfile> Profiles, int Total)> GetPagedProfilesAsync(
            int? minElo = null,
            int? maxElo = null,
            decimal? minTrustScore = null,
            string? searchTerm = null,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default);
    }
}
