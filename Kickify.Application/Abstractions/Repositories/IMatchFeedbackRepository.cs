using Kickify.Application.Abstractions.Persistence;
using Kickify.Domain.Entities;

namespace Kickify.Application.Abstractions.Repositories;

public interface IMatchFeedbackRepository : IGenericRepository<MatchFeedback>
{
    Task<MatchFeedback?> GetByMatchAndUsersAsync(Guid matchId, Guid reviewerId, Guid revieweeId, CancellationToken cancellationToken = default);
    Task<bool> HasUserReviewedAsync(Guid matchId, Guid reviewerId, Guid revieweeId, CancellationToken cancellationToken = default);
    Task<List<MatchFeedback>> GetFeedbacksByMatchAsync(Guid matchId, CancellationToken cancellationToken = default);
    Task<List<Guid>> GetMatchesReviewedByUserAsync(Guid userId, List<Guid> matchIds, CancellationToken cancellationToken = default);
}
