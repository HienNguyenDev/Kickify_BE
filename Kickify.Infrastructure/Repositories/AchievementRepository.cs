using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Entities;
using Kickify.Infrastructure.Database;
using Kickify.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kickify.Infrastructure.Repositories;

public class AchievementRepository : GenericRepository<Achievement>, IAchievementRepository
{
    public AchievementRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Achievement?> GetByIdAsync(Guid achievementId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(a => a.AchievementId == achievementId, cancellationToken);
    }

    public async Task<Achievement?> GetByIdIncludeDeletedAsync(Guid achievementId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(a => a.AchievementId == achievementId, cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();

        if (excludeId.HasValue)
        {
            query = query.Where(a => a.AchievementId != excludeId.Value);
        }

        return await query.AnyAsync(a => a.Name.ToLower() == name.ToLower(), cancellationToken);
    }

    public async Task<(IEnumerable<Achievement> Items, int Total)> GetAllPagedAsync(
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsNoTracking().OrderByDescending(a => a.CreatedAt);

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<List<(Achievement Achievement, DateTime? EarnedAt)>> GetAllWithUserProgressAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var result = await _dbSet
            .AsNoTracking()
            .GroupJoin(
                _context.PlayerAchievements.Where(pa => pa.UserId == userId),
                a => a.AchievementId,
                pa => pa.AchievementId,
                (achievement, playerAchievements) => new { achievement, playerAchievements })
            .SelectMany(
                x => x.playerAchievements.DefaultIfEmpty(),
                (x, pa) => new
                {
                    Achievement = x.achievement,
                    EarnedAt = pa != null ? (DateTime?)pa.EarnedAt : null
                })
            .OrderBy(x => x.Achievement.CriteriaType)
            .ThenBy(x => x.Achievement.CriteriaValue)
            .ToListAsync(cancellationToken);

        return result.Select(x => (x.Achievement, x.EarnedAt)).ToList();
    }

    public async Task<bool> HasUserClaimedAsync(
        Guid userId,
        Guid achievementId,
        CancellationToken cancellationToken = default)
    {
        return await _context.PlayerAchievements
            .AnyAsync(pa => pa.UserId == userId && pa.AchievementId == achievementId, cancellationToken);
    }

    public async Task AddPlayerAchievementAsync(
        PlayerAchievement playerAchievement,
        CancellationToken cancellationToken = default)
    {
        await _context.PlayerAchievements.AddAsync(playerAchievement, cancellationToken);
    }
}
