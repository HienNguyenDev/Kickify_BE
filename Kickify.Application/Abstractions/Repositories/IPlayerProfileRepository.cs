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
            List<string>? positions = null,
            string? preferredFoot = null,
            bool highFormOnly = false,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get top N players by ELO rating with latest ELO change
        /// </summary>
        Task<List<(PlayerProfile Profile, int LatestEloChange)>> GetTopPlayersByEloWithChangeAsync(int count, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get player's rank based on ELO (1 = highest ELO)
        /// </summary>
        Task<int> GetPlayerRankByEloAsync(Guid userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get total count of all players
        /// </summary>
        Task<int> GetTotalPlayersCountAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Get latest ELO change for a user
        /// </summary>
        Task<int> GetLatestEloChangeAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
