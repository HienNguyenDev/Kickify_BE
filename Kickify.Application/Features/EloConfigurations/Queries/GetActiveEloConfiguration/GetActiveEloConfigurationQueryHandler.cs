using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Domain.Common;
using Kickify.Domain.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kickify.Application.Features.EloConfigurations.Queries.GetActiveEloConfiguration;

public class GetActiveEloConfigurationQueryHandler : IQueryHandler<GetActiveEloConfigurationQuery, GetActiveEloConfigurationResponse>
{
    private readonly IApplicationDbContext _dbContext;

    public GetActiveEloConfigurationQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<GetActiveEloConfigurationResponse>> Handle(GetActiveEloConfigurationQuery request, CancellationToken cancellationToken)
    {
        var activeConfig = await _dbContext.EloConfigurations
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.EffectiveFrom)
            .FirstOrDefaultAsync(cancellationToken);

        if (activeConfig is null)
        {
            return Result.Failure<GetActiveEloConfigurationResponse>(EloConfigurationErrors.ActiveNotFound);
        }

        var response = new GetActiveEloConfigurationResponse(
            activeConfig.ConfigId,
            activeConfig.K1MatchResult,
            activeConfig.K2FeedbackSentiment,
            activeConfig.K3WinStreak,
            activeConfig.K4Contribution,
            activeConfig.K5Trust,
            activeConfig.IsActive,
            activeConfig.UpdatedAt);

        return Result.Success(response);
    }
}
