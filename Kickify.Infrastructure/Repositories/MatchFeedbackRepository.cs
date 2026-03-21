using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Entities;
using Kickify.Infrastructure.Database;
using Kickify.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kickify.Infrastructure.Repositories;

public class MatchFeedbackRepository : GenericRepository<MatchFeedback>, IMatchFeedbackRepository
{
    public MatchFeedbackRepository(ApplicationDbContext context) : base(context) { }

    public async Task<MatchFeedback?> GetByMatchAndUsersAsync(Guid matchId, Guid reviewerId, Guid revieweeId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(f => f.MatchId == matchId && f.ReviewerId == reviewerId && f.RevieweeId == revieweeId, cancellationToken);
    }

    public async Task<bool> HasUserReviewedAsync(Guid matchId, Guid reviewerId, Guid revieweeId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(f => f.MatchId == matchId && f.ReviewerId == reviewerId && f.RevieweeId == revieweeId, cancellationToken);
    }

    public async Task<List<MatchFeedback>> GetFeedbacksByMatchAsync(Guid matchId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(f => f.MatchId == matchId).ToListAsync(cancellationToken);
    }

    public async Task<List<Guid>> GetMatchesReviewedByUserAsync(Guid userId, List<Guid> matchIds, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(f => f.ReviewerId == userId && matchIds.Contains(f.MatchId))
            .Select(f => f.MatchId)
            .Distinct()
            .ToListAsync(cancellationToken);
    }
}
