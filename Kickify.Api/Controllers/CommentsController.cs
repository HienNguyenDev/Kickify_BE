using Kickify.Api.Extensions;
using Kickify.Api.Requests;
using Kickify.Application.Features.Comments.Commands.CreateComment;
using Kickify.Application.Features.Comments.Commands.DeleteComment;
using Kickify.Application.Features.Comments.Commands.LikeComment;
using Kickify.Application.Features.Comments.Commands.UpdateComment;
using Kickify.Application.Features.Comments.Queries.GetCommentsByPost;
using Kickify.Application.Features.Comments.Queries.GetRepliesByComment;
using Kickify.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kickify.Api.Controllers;

[Route("api/")]
[ApiController]
[Authorize]
public class CommentsController : ControllerBase
{
    private readonly ISender _mediator;

    public CommentsController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("comments/{postId:guid}")]
    [AllowAnonymous]
    public async Task<IResult> GetCommentsByPost(Guid postId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var query = new GetCommentsByPostQuery { PostId = postId, Page = page, PageSize = pageSize };
        Result<GetCommentsByPostQueryResponse> result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    [HttpGet("comments/{commentId:guid}/replies")]
    [AllowAnonymous]
    public async Task<IResult> GetRepliesByComment(Guid commentId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var query = new GetRepliesByCommentQuery { CommentId = commentId, Page = page, PageSize = pageSize };
        Result<GetRepliesByCommentQueryResponse> result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    [HttpPost("comments/{postId:guid}")]
    public async Task<IResult> CreateComment(Guid postId, [FromBody] CreateCommentRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateCommentCommand { PostId = postId, ParentCommentId = request.ParentCommentId, Content = request.Content };
        Result<CreateCommentCommandResponse> result = await _mediator.Send(command, cancellationToken);
        return result.MatchCreated(r => $"/api/comments/{r.CommentId}");
    }

    [HttpPut("comments/{commentId:guid}")]
    public async Task<IResult> UpdateComment(Guid commentId, [FromBody] UpdateCommentRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateCommentCommand { CommentId = commentId, Content = request.Content };
        Result<UpdateCommentCommandResponse> result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    [HttpDelete("comments/{commentId:guid}")]
    public async Task<IResult> DeleteComment(Guid commentId, CancellationToken cancellationToken)
    {
        var command = new DeleteCommentCommand { CommentId = commentId };
        Result<DeleteCommentCommandResponse> result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    [HttpPost("comments/{commentId:guid}/like")]
    public async Task<IResult> LikeComment(Guid commentId, CancellationToken cancellationToken)
    {
        var command = new LikeCommentCommand { CommentId = commentId };
        Result<LikeCommentCommandResponse> result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }
}