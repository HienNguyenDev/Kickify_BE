using Kickify.Application.Abstractions.Persistence;
using Kickify.Domain.Entities;

namespace Kickify.Application.Abstractions.Repositories;

public interface ICommentLikeRepository : IGenericRepository<CommentLike>
{
    Task<CommentLike?> GetByCommentAndUserAsync(Guid commentId, Guid userId, CancellationToken cancellationToken = default);
    Task<bool> IsCommentLikedByUserAsync(Guid commentId, Guid userId, CancellationToken cancellationToken = default);
    Task<HashSet<Guid>> GetLikedCommentIdsByUserAsync(IEnumerable<Guid> commentIds, Guid userId, CancellationToken cancellationToken = default);
}
