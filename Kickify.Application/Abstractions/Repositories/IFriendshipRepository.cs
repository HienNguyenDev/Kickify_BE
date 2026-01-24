using Kickify.Application.Abstractions.Persistence;
using Kickify.Domain.Entities;

namespace Kickify.Application.Abstractions.Repositories;

public interface IFriendshipRepository : IGenericRepository<Friendship>
{
    Task<Friendship?> GetFriendshipAsync(Guid userId1, Guid userId2, CancellationToken cancellationToken = default);
    Task<Friendship?> GetFriendshipIncludeDeletedAsync(Guid userId1, Guid userId2, CancellationToken cancellationToken = default);
    Task<bool> AreFriendsAsync(Guid userId1, Guid userId2, CancellationToken cancellationToken = default);
    Task<(IEnumerable<Friendship> Friendships, int Total)> GetFriendsAsync(Guid userId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<(IEnumerable<Friendship> Requests, int Total)> GetPendingRequestsAsync(Guid userId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<(IEnumerable<Friendship> Requests, int Total)> GetSentRequestsAsync(Guid userId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
}
