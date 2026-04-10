using Kickify.Api.Extensions;
using Kickify.Api.Requests;
using Kickify.Application.Features.EloConfigurations.Commands.UpdateActiveEloConfiguration;
using Kickify.Application.Features.EloConfigurations.Queries.GetActiveEloConfiguration;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kickify.Api.Controllers;

[ApiController]
[Route("api/elo-configurations")]
[Authorize(Roles = "Admin")]
public class EloConfigurationsController : ControllerBase
{
    private readonly ISender _sender;

    public EloConfigurationsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Get active ELO configuration.
    /// </summary>
    [HttpGet("active")]
    public async Task<IResult> GetActive(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetActiveEloConfigurationQuery(), cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// Update active ELO configuration.
    /// </summary>
    [HttpPut("active")]
    public async Task<IResult> UpdateActive([FromBody] UpdateActiveEloConfigurationRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateActiveEloConfigurationCommand(
            request.K1MatchResult,
            request.K2FeedbackSentiment,
            request.K3WinRate,
            request.K4Contribution,
            request.K5Trust);

        var result = await _sender.Send(command, cancellationToken);
        return result.MatchOk();
    }
}
