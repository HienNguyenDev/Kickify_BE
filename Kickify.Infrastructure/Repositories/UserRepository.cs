using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Infrastructure.Database;
using Kickify.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kickify.Infrastructure.Repositories;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<bool> IsEmailExistsAsync(string email)
    {
        return await _dbSet
            .AnyAsync(u => u.Email == email);
    }

    public async Task<IEnumerable<User>> GetByIdsAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default)
    {
        var idList = userIds.ToList();
        return await _dbSet
            .AsNoTracking()
            .Where(u => idList.Contains(u.UserId))
            .ToListAsync(cancellationToken);
    }

    public async Task<(IEnumerable<User> Users, int Total)> GetPagedUsersAsync(
        UserRole? role = null,
        bool? isActive = null,
        string? searchTerm = null,
        int page = 1,
        int pageSize = 10,
        bool includeDeleted = false,
        CancellationToken cancellationToken = default)
    {
        IQueryable<User> query = _dbSet.AsNoTracking();

        if (role.HasValue)
        {
            query = query.Where(u => u.Role == role.Value);
        }

        if (isActive.HasValue)
        {
            query = query.IgnoreQueryFilters().Where(u => u.IsActive == isActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var lowerSearchTerm = searchTerm.ToLower();
            query = query.Where(u =>
                u.Email.ToLower().Contains(lowerSearchTerm) ||
                (u.FullName != null && u.FullName.ToLower().Contains(lowerSearchTerm)) ||
                (u.Phone != null && u.Phone.Contains(searchTerm))
            );
        }

        var total = await query.CountAsync(cancellationToken);

        var users = await query
            .Include(u => u.PlayerProfile)
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (users, total);
    }

    public async Task<User?> GetUserWithDetailsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(u => u.PlayerProfile)
            .Include(u => u.NotificationPreference)
            .Include(u => u.PlayerAchievements)
                .ThenInclude(pa => pa.Achievement)
            .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);
    }

    public async Task<User?> GetUserByEmailIgnoreFilterAsync(string email)
    {
        return await _dbSet
            .IgnoreQueryFilters()  
            .FirstOrDefaultAsync(u => u.Email == email);
    }
}

