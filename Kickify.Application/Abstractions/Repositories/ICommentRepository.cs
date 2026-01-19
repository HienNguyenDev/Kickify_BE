using Kickify.Application.Abstractions.Persistence;
using Kickify.Domain.Entities;

namespace Kickify.Application.Abstractions.Repositories;

public interface ICommentRepository : IGenericRepository<Comment>
{
    Task<Comment?> GetCommentWithDetailsAsync(Guid commentId, CancellationToken cancellationToken = default);
    Task<(IEnumerable<Comment> Comments, int Total)> GetCommentsByPostAsync(Guid postId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<(IEnumerable<Comment> Replies, int Total)> GetRepliesByCommentAsync(Guid commentId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
}
