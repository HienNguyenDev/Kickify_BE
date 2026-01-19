using Kickify.Application.Abstractions.Persistence;
using Kickify.Domain.Entities;

namespace Kickify.Application.Abstractions.Repositories;

public interface ICommentLikeRepository : IGenericRepository<CommentLike>
{
    Task<CommentLike?> GetByCommentAndUserAsync(Guid commentId, Guid userId, CancellationToken cancellationToken = default);
}
