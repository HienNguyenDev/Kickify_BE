using Kickify.Api.Extensions;
using Kickify.Application.Features.MatchFeedbacks.Commands.CreateMatchFeedback;
using Kickify.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kickify.Api.Controllers;

[Route("api/match-feedbacks")]
[ApiController]
[Authorize]
public class MatchFeedbacksController : ControllerBase
{
    private readonly ISender _mediator;

    public MatchFeedbacksController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// T?o feedback cho m?t ng??i ch?i khác sau khi tr?n ??u hoàn thành
    /// </summary>
    [HttpPost]
    public async Task<IResult> CreateMatchFeedback([FromBody] CreateMatchFeedbackCommand command, CancellationToken cancellationToken)
    {
        Result<CreateMatchFeedbackCommandResponse> result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }
}
