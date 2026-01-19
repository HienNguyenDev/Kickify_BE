using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Entities;
using Kickify.Infrastructure.Database;
using Kickify.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kickify.Infrastructure.Repositories;

public class CommentRepository : GenericRepository<Comment>, ICommentRepository
{
    public CommentRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Comment?> GetCommentWithDetailsAsync(Guid commentId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Include(c => c.User).Include(c => c.Replies.Where(r => r.IsActive)).ThenInclude(r => r.User).FirstOrDefaultAsync(c => c.CommentId == commentId && c.IsActive, cancellationToken);
    }

    public async Task<(IEnumerable<Comment> Comments, int Total)> GetCommentsByPostAsync(Guid postId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Include(c => c.User).Where(c => c.PostId == postId && c.ParentCommentId == null && c.IsActive);
        var total = await query.CountAsync(cancellationToken);
        var comments = await query.OrderByDescending(c => c.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return (comments, total);
    }

    public async Task<(IEnumerable<Comment> Replies, int Total)> GetRepliesByCommentAsync(Guid commentId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Include(c => c.User).Where(c => c.ParentCommentId == commentId && c.IsActive);
        var total = await query.CountAsync(cancellationToken);
        var replies = await query.OrderBy(c => c.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return (replies, total);
    }
}
