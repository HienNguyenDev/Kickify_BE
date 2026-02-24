using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Domain.Entities;

namespace Kickify.Application.Abstractions.Repositories
{
    public interface IPostRepository : IGenericRepository<Post>
    {
        Task<Post?> GetPostWithDetailsAsync(Guid postId, CancellationToken cancellationToken = default);
        Task<(IEnumerable<Post> Posts, int Total)> GetPagedPostsAsync(
            Guid? userId = null,
            string? searchTerm = null,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default);
    }
}
