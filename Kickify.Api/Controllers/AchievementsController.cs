using Kickify.Api.Extensions;
using Kickify.Application.Features.Achievements.Queries.GetMyAchievements;
using Kickify.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kickify.Api.Controllers;

[Route("api/me/achievements")]
[ApiController]
[Authorize]
public class AchievementsController : ControllerBase
{
    private readonly ISender _mediator;

    public AchievementsController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IResult> GetMyAchievements(CancellationToken cancellationToken)
    {
        var query = new GetMyAchievementsQuery();
        Result<GetMyAchievementsResponse> result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }
}
