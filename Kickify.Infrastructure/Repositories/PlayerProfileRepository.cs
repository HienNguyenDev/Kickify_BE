using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Entities;
using Kickify.Infrastructure.Database;
using Kickify.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kickify.Infrastructure.Repositories
{
    public class PlayerProfileRepository : GenericRepository<PlayerProfile>, IPlayerProfileRepository
    {
        public PlayerProfileRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<PlayerProfile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);
        }

        public async Task<bool> ExistsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AnyAsync(p => p.UserId == userId, cancellationToken);
        }

        public async Task<(IEnumerable<PlayerProfile> Profiles, int Total)> GetPagedProfilesAsync(
            int? minElo = null,
            int? maxElo = null,
            decimal? minTrustScore = null,
            string? searchTerm = null,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            // Note: Global query filter already excludes soft-deleted profiles
            var query = _dbSet
                .AsNoTracking()
                .Include(p => p.User)
                .AsQueryable();

            // Filter by ELO range
            if (minElo.HasValue)
            {
                query = query.Where(p => p.CurrentElo >= minElo.Value);
            }

            if (maxElo.HasValue)
            {
                query = query.Where(p => p.CurrentElo <= maxElo.Value);
            }

            // Filter by minimum trust score
            if (minTrustScore.HasValue)
            {
                query = query.Where(p => p.TrustScore >= minTrustScore.Value);
            }

            // Search by user's full name or email
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var lowerSearchTerm = searchTerm.ToLower();
                query = query.Where(p =>
                    (p.User.FullName != null && p.User.FullName.ToLower().Contains(lowerSearchTerm)) ||
                    p.User.Email.ToLower().Contains(lowerSearchTerm)
                );
            }

            var total = await query.CountAsync(cancellationToken);

            var profiles = await query
                .OrderByDescending(p => p.CurrentElo)
                .ThenByDescending(p => p.TrustScore)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (profiles, total);
        }
    }
}
