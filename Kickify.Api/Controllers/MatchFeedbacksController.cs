using Kickify.Api.Extensions;
using Kickify.Api.Requests;
using Kickify.Application.Abstractions.Services;
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
    private readonly ISentimentAnalysisService _sentimentAnalysisService;

    public MatchFeedbacksController(
        ISender mediator,
        ISentimentAnalysisService sentimentAnalysisService)
    {
        _mediator = mediator;
        _sentimentAnalysisService = sentimentAnalysisService;
    }

    /// <summary>
    /// Gửi feedback trong giai đoạn Reviewing: một request có thể chứa nhiều người nhận (mỗi RevieweeId một lần trong payload).
    /// Mỗi cặp reviewer → một reviewee chỉ được một feedback cho trận đó; có thể gọi thêm request để review các đối thủ/đồng đội khác chưa được gửi.
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

    /// <summary>
    /// Generate AI feedback suggestions by star rating and optional role.
    /// </summary>
    [HttpPost("generate-suggestions")]
    public async Task<IResult> GenerateFeedbackSuggestions(
        [FromBody] GenerateFeedbackSuggestionRequest request,
        CancellationToken cancellationToken)
    {
        var aiRequest = new FeedbackSuggestionRequest(
            StarRating: request.StarRating,
            Count: request.Count,
            Role: request.Role
        );

        var aiResponse = await _sentimentAnalysisService.GenerateFeedbackSuggestionsAsync(
            aiRequest,
            cancellationToken);

        if (aiResponse is null)
        {
            return Results.Problem(
                title: "AI feedback generation is unavailable",
                detail: "Cannot generate feedback suggestions at the moment.",
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }

        return Results.Ok(aiResponse);
    }
}
