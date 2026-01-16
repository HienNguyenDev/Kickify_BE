using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Entities;
using Kickify.Infrastructure.Database;
using Kickify.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kickify.Infrastructure.Repositories;

public class PostRepository : GenericRepository<Post>, IPostRepository
{
    public PostRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Post?> GetPostWithDetailsAsync(Guid postId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.User)
            .Include(p => p.PostMedia.OrderBy(m => m.DisplayOrder))
            .FirstOrDefaultAsync(p => p.PostId == postId, cancellationToken);
    }

    public async Task<(IEnumerable<Post> Posts, int Total)> GetPagedPostsAsync(
        Guid? userId = null,
        string? searchTerm = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Include(p => p.User)
            .Include(p => p.PostMedia.OrderBy(m => m.DisplayOrder))
            .AsQueryable();

        if (userId.HasValue)
        {
            query = query.Where(p => p.UserId == userId.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(p => p.Content.Contains(searchTerm));
        }

        var total = await query.CountAsync(cancellationToken);

        var posts = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (posts, total);
    }
}
