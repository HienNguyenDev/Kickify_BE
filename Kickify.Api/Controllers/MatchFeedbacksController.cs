using Kickify.Api.Extensions;
using Kickify.Api.Requests;
using Kickify.Application.Features.MatchFeedbacks.Commands.CreateMatchFeedback;
using Kickify.Application.Features.MatchFeedbacks.Commands.RespondToFeedback;
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
    /// Gui tat ca feedback cua 1 reviewer danh cho cac dong doi trong 1 request.
    /// Vi du tran 5v5: gui 4 feedback cua 1 nguoi toi 4 nguoi con lai.
    /// </summary>
    [HttpPost]
    public async Task<IResult> CreateMatchFeedback([FromBody] CreateMatchFeedbackRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateMatchFeedbackCommand
        {
            MatchId = request.MatchId,
            ReviewerId = request.ReviewerId,
            Feedbacks = request.Feedbacks.Select(f => new FeedbackItemDto
            {
                FeedbackId = f.FeedbackId,
                RevieweeId = f.RevieweeId,
                Comment = f.Comment,
                Rating = f.Rating
            }).ToList()
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// Gui phan hoi cho feedback da nhan.
    /// </summary>
    [HttpPost("{feedbackId:guid}/response")]
    public async Task<IResult> RespondToFeedback(
        Guid feedbackId,
        [FromBody] RespondToFeedbackRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RespondToFeedbackCommand(feedbackId, request.Response);
        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }
}
