using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Entities;
using Kickify.Infrastructure.Database;
using Kickify.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kickify.Infrastructure.Repositories;

public class PostLikeRepository : GenericRepository<PostLike>, IPostLikeRepository
{
    public PostLikeRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<PostLike?> GetByPostAndUserAsync(Guid postId, Guid userId)
    {
        return await _dbSet.FirstOrDefaultAsync(pl => pl.PostId == postId && pl.UserId == userId);
    }
}
