using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kickify.Application.Features.PlayerProfiles.Queries.GetMyEloBreakdown;

public class GetMyEloBreakdownQueryHandler : IQueryHandler<GetMyEloBreakdownQuery, IReadOnlyList<EloBreakdownItemResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserContext _userContext;

    public GetMyEloBreakdownQueryHandler(IApplicationDbContext dbContext, IUserContext userContext)
    {
        _dbContext = dbContext;
        _userContext = userContext;
    }

    /// <summary>
    /// Get ELO breakdown history for current player.
    /// </summary>
    public async Task<Result<IReadOnlyList<EloBreakdownItemResponse>>> Handle(GetMyEloBreakdownQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.EloHistories
            .AsNoTracking()
            .Where(x => x.UserId == _userContext.UserId);

        if (request.MatchId.HasValue)
        {
            query = query.Where(x => x.MatchId == request.MatchId.Value);
        }

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Take(20)
            .Select(x => new EloBreakdownItemResponse(
                x.MatchId,
                x.EloBefore,
                x.EloAfter,
                x.EloChange,
                x.K1MatchResultComponent,
                x.K2FeedbackSentimentComponent,
                x.K3WinStreakComponent,
                x.K4ContributionComponent,
                x.K5TrustComponent,
                x.CalculationDetails,
                x.CreatedAt))
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<EloBreakdownItemResponse>>(items);
    }
}
