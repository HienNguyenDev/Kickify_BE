using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Domain.Common;
using Kickify.Domain.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kickify.Application.Features.EloConfigurations.Commands.UpdateActiveEloConfiguration;

public class UpdateActiveEloConfigurationCommandHandler : ICommandHandler<UpdateActiveEloConfigurationCommand, UpdateActiveEloConfigurationResponse>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateActiveEloConfigurationCommandHandler(IApplicationDbContext dbContext, IUnitOfWork unitOfWork)
    {
        _dbContext = dbContext;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UpdateActiveEloConfigurationResponse>> Handle(UpdateActiveEloConfigurationCommand request, CancellationToken cancellationToken)
    {
        var activeConfig = await _dbContext.EloConfigurations
            .FirstOrDefaultAsync(x => x.IsActive, cancellationToken);

        if (activeConfig is null)
        {
            return Result.Failure<UpdateActiveEloConfigurationResponse>(EloConfigurationErrors.ActiveNotFound);
        }

        activeConfig.K1MatchResult = request.K1MatchResult;
        activeConfig.K2FeedbackSentiment = request.K2FeedbackSentiment;
        activeConfig.K3WinRate = request.K3WinRate;
        activeConfig.K4Contribution = request.K4Contribution;
        activeConfig.K5Trust = request.K5Trust;

        _dbContext.EloConfigurations.Update(activeConfig);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new UpdateActiveEloConfigurationResponse(
            activeConfig.ConfigId,
            activeConfig.K1MatchResult,
            activeConfig.K2FeedbackSentiment,
            activeConfig.K3WinRate,
            activeConfig.K4Contribution,
            activeConfig.K5Trust,
            activeConfig.IsActive,
            activeConfig.UpdatedAt);

        return Result.Success(response);
    }
}
