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
}
