using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Entities;
using Kickify.Infrastructure.Database;
using Kickify.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kickify.Infrastructure.Repositories
{
    public class MatchPresetRepository : GenericRepository<MatchPreset>, IMatchPresetRepository
    {
        public MatchPresetRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<MatchPreset?> GetByIdAsync(Guid presetId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(p => p.PresetId == presetId, cancellationToken);
        }

        public async Task<MatchPreset?> GetByIdWithDetailsAsync(Guid presetId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(p => p.User)
                .Include(p => p.Field)
                    .ThenInclude(f => f!.Venue)
                .FirstOrDefaultAsync(p => p.PresetId == presetId, cancellationToken);
        }

        public async Task<List<MatchPreset>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(p => p.Field)
                    .ThenInclude(f => f!.Venue)
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<(IEnumerable<MatchPreset> Presets, int Total)> GetAllPagedAsync(
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var query = _dbSet
                .AsNoTracking()
                .Include(p => p.User)
                .Include(p => p.Field)
                    .ThenInclude(f => f!.Venue)
                .OrderByDescending(p => p.CreatedAt);

            var total = await query.CountAsync(cancellationToken);

            var presets = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (presets, total);
        }

        public async Task<(IEnumerable<MatchPreset> Presets, int Total)> GetByUserIdPagedAsync(
            Guid userId,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var query = _dbSet
                .AsNoTracking()
                .Include(p => p.Field)
                    .ThenInclude(f => f!.Venue)
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt);

            var total = await query.CountAsync(cancellationToken);

            var presets = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (presets, total);
        }
    }
}
