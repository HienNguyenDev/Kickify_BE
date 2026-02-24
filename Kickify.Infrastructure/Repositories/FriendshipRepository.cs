using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Infrastructure.Database;
using Kickify.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kickify.Infrastructure.Repositories;

public class FriendshipRepository : GenericRepository<Friendship>, IFriendshipRepository
{
    public FriendshipRepository(ApplicationDbContext context) : base(context) { }

    public new async Task<Friendship?> GetByIdAsync(object id)
    {
        if (id is Guid friendshipId)
        {
            return await _dbSet.FirstOrDefaultAsync(f => f.FriendshipId == friendshipId);
        }
        return null;
    }

    public async Task<Friendship?> GetFriendshipAsync(Guid userId1, Guid userId2, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(f => (f.RequesterId == userId1 && f.AddresseeId == userId2) || (f.RequesterId == userId2 && f.AddresseeId == userId1), cancellationToken);
    }

    public async Task<Friendship?> GetFriendshipIncludeDeletedAsync(Guid userId1, Guid userId2, CancellationToken cancellationToken = default)
    {
        return await _dbSet.IgnoreQueryFilters().FirstOrDefaultAsync(f => (f.RequesterId == userId1 && f.AddresseeId == userId2) || (f.RequesterId == userId2 && f.AddresseeId == userId1), cancellationToken);
    }

    public async Task<bool> AreFriendsAsync(Guid userId1, Guid userId2, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(f => ((f.RequesterId == userId1 && f.AddresseeId == userId2) || (f.RequesterId == userId2 && f.AddresseeId == userId1)) && f.Status == FriendshipStatus.Accepted, cancellationToken);
    }

    public async Task<(IEnumerable<Friendship> Friendships, int Total)> GetFriendsAsync(Guid userId, string? searchTerm = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Include(f => f.Requester).ThenInclude(u => u.PlayerProfile)
            .Include(f => f.Addressee).ThenInclude(u => u.PlayerProfile)
            .Where(f => (f.RequesterId == userId || f.AddresseeId == userId) && f.Status == FriendshipStatus.Accepted);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var searchLower = searchTerm.ToLower();
            query = query.Where(f =>
                (f.RequesterId == userId && f.Addressee.FullName != null && f.Addressee.FullName.ToLower().Contains(searchLower)) ||
                (f.AddresseeId == userId && f.Requester.FullName != null && f.Requester.FullName.ToLower().Contains(searchLower)));
        }

        var total = await query.CountAsync(cancellationToken);
        var friendships = await query.OrderByDescending(f => f.RespondedAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return (friendships, total);
    }

    public async Task<(IEnumerable<Friendship> Requests, int Total)> GetPendingRequestsAsync(Guid userId, string? searchTerm = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Include(f => f.Requester).ThenInclude(u => u.PlayerProfile)
            .Where(f => f.AddresseeId == userId && f.Status == FriendshipStatus.Pending);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var searchLower = searchTerm.ToLower();
            query = query.Where(f => f.Requester.FullName != null && f.Requester.FullName.ToLower().Contains(searchLower));
        }

        var total = await query.CountAsync(cancellationToken);
        var requests = await query.OrderByDescending(f => f.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return (requests, total);
    }

    public async Task<(IEnumerable<Friendship> Requests, int Total)> GetSentRequestsAsync(Guid userId, string? searchTerm = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Include(f => f.Addressee).ThenInclude(u => u.PlayerProfile)
            .Where(f => f.RequesterId == userId && f.Status == FriendshipStatus.Pending);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var searchLower = searchTerm.ToLower();
            query = query.Where(f => f.Addressee.FullName != null && f.Addressee.FullName.ToLower().Contains(searchLower));
        }

        var total = await query.CountAsync(cancellationToken);
        var requests = await query.OrderByDescending(f => f.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return (requests, total);
    }
}
