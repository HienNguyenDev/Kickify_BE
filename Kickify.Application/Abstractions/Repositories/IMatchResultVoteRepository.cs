using Kickify.Application.Abstractions.Persistence;

namespace Kickify.Application.Abstractions.Repositories;

public interface IMatchResultVoteRepository : IGenericRepository<Domain.Entities.MatchResultVote>
{
    Task<bool> HasUserVotedAsync(Guid roomId, Guid userId, CancellationToken cancellationToken = default);
    Task<List<Domain.Entities.MatchResultVote>> GetVotesByRoomAsync(Guid roomId, CancellationToken cancellationToken = default);
    Task<int> GetVoteCountByRoomAsync(Guid roomId, CancellationToken cancellationToken = default);
}
