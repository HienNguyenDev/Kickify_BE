using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Entities;
using Kickify.Infrastructure.Database;
using Kickify.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kickify.Infrastructure.Repositories;

public class CommentLikeRepository : GenericRepository<CommentLike>, ICommentLikeRepository
{
    public CommentLikeRepository(ApplicationDbContext context) : base(context) { }

    public async Task<CommentLike?> GetByCommentAndUserAsync(Guid commentId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(cl => cl.CommentId == commentId && cl.UserId == userId, cancellationToken);
    }

    public async Task<bool> IsCommentLikedByUserAsync(Guid commentId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(cl => cl.CommentId == commentId && cl.UserId == userId, cancellationToken);
    }

    public async Task<HashSet<Guid>> GetLikedCommentIdsByUserAsync(IEnumerable<Guid> commentIds, Guid userId, CancellationToken cancellationToken = default)
    {
        var likedCommentIds = await _dbSet
            .Where(cl => commentIds.Contains(cl.CommentId) && cl.UserId == userId)
            .Select(cl => cl.CommentId)
            .ToListAsync(cancellationToken);

        return likedCommentIds.ToHashSet();
    }
}
