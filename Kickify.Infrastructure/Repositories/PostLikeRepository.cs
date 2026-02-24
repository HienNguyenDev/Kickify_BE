using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Entities;
using Kickify.Infrastructure.Database;
using Kickify.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kickify.Infrastructure.Repositories;

public class PostLikeRepository : GenericRepository<PostLike>, IPostLikeRepository
{
    public PostLikeRepository(ApplicationDbContext context) : base(context) { }

    public async Task<PostLike?> GetByPostAndUserAsync(Guid postId, Guid userId)
    {
        return await _dbSet.FirstOrDefaultAsync(pl => pl.PostId == postId && pl.UserId == userId);
    }

    public async Task<bool> IsPostLikedByUserAsync(Guid postId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(pl => pl.PostId == postId && pl.UserId == userId, cancellationToken);
    }

    public async Task<HashSet<Guid>> GetLikedPostIdsByUserAsync(IEnumerable<Guid> postIds, Guid userId, CancellationToken cancellationToken = default)
    {
        var likedPostIds = await _dbSet
            .Where(pl => postIds.Contains(pl.PostId) && pl.UserId == userId)
            .Select(pl => pl.PostId)
            .ToListAsync(cancellationToken);

        return likedPostIds.ToHashSet();
    }
}
