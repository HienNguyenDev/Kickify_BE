using Kickify.Api.Extensions;
using Kickify.Api.Requests;
using Kickify.Application.Features.MatchFeedbacks.Commands.CreateMatchFeedback;
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
    /// G?i t?t c? feedback c?a các reviewer dành cho 1 reviewee trong 1 request.
    /// Ví d? tr?n 5v5: g?i 4 feedback t? 4 ng??i còn l?i cho 1 ng??i.
    /// </summary>
    [HttpPost]
    public async Task<IResult> CreateMatchFeedback([FromBody] CreateMatchFeedbackRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateMatchFeedbackCommand
        {
            MatchId = request.MatchId,
            RevieweeId = request.RevieweeId,
            Feedbacks = request.Feedbacks.Select(f => new FeedbackItemDto
            {
                FeedbackId = f.FeedbackId,
                ReviewerId = f.ReviewerId,
                Comment = f.Comment,
                Rating = f.Rating
            }).ToList()
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }
}

