using Kickify.Application.Abstractions.Persistence;
using Kickify.Domain.Entities;

namespace Kickify.Application.Abstractions.Repositories;

public interface IAchievementRepository : IGenericRepository<Achievement>
{
    Task<Achievement?> GetByIdAsync(Guid achievementId, CancellationToken cancellationToken = default);

    Task<Achievement?> GetByIdIncludeDeletedAsync(Guid achievementId, CancellationToken cancellationToken = default);

    Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default);

    Task<(IEnumerable<Achievement> Items, int Total)> GetAllPagedAsync(
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all achievements with left join to PlayerAchievements for a specific user.
    /// Returns all achievements with isUnlocked and earnedAt populated.
    /// </summary>
    Task<List<(Achievement Achievement, DateTime? EarnedAt)>> GetAllWithUserProgressAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
