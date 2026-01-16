using Kickify.Application.Abstractions.Persistence;
using Kickify.Domain.Entities;

namespace Kickify.Application.Abstractions.Repositories;

public interface IPostLikeRepository : IGenericRepository<PostLike>
{
    Task<PostLike?> GetByPostAndUserAsync(Guid postId, Guid userId);
}
