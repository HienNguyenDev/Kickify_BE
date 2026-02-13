using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Entities;
using Kickify.Infrastructure.Database;
using Kickify.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kickify.Infrastructure.Repositories;

public class MatchResultVoteRepository : GenericRepository<MatchResultVote>, IMatchResultVoteRepository
{
    public MatchResultVoteRepository(ApplicationDbContext context) : base(context) { }

    public async Task<bool> HasUserVotedAsync(Guid roomId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(v => v.RoomId == roomId && v.UserId == userId, cancellationToken);
    }

    public async Task<List<MatchResultVote>> GetVotesByRoomAsync(Guid roomId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(v => v.RoomId == roomId)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetVoteCountByRoomAsync(Guid roomId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.CountAsync(v => v.RoomId == roomId, cancellationToken);
    }
}
