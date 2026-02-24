using Kickify.Api.Extensions;
using Kickify.Api.Requests;
using Kickify.Application.Abstractions.Services;
using Kickify.Application.Features.Posts.Commands.CreatePost;
using Kickify.Application.Features.Posts.Commands.DeletePost;
using Kickify.Application.Features.Posts.Commands.LikePost;
using Kickify.Application.Features.Posts.Commands.UpdatePost;
using Kickify.Application.Features.Posts.Queries.GetAllPosts;
using Kickify.Application.Features.Posts.Queries.GetPostById;
using Kickify.Domain.Common;
using Kickify.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kickify.Api.Controllers;

[Route("api/posts")]
[ApiController]
[Authorize]
public class PostsController : ControllerBase
{
    private readonly ISender _mediator;

    public PostsController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IResult> GetAllPosts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? userId = null,
        [FromQuery] string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllPostsQuery
        {
            Page = page,
            PageSize = pageSize,
            UserId = userId,
            SearchTerm = searchTerm
        };
        Result<GetAllPostsQueryResponse> result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    [HttpGet("{postId:guid}")]
    public async Task<IResult> GetPostById(Guid postId, CancellationToken cancellationToken)
    {
        var query = new GetPostByIdQuery { PostId = postId };
        Result<GetPostByIdQueryResponse> result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IResult> CreatePost([FromForm] CreatePostRequest request, CancellationToken cancellationToken)
    {
        var files = new List<FileUploadRequest>();

        if (request.Files is not null && request.Files.Count > 0)
        {
            foreach (var file in request.Files)
            {
                files.Add(new FileUploadRequest(
                    file.OpenReadStream(),
                    file.FileName,
                    file.ContentType,
                    file.Length));
            }
        }

        var command = new CreatePostCommand
        {
            Content = request.Content,
            Files = files
        };

        Result<CreatePostCommandResponse> result = await _mediator.Send(command, cancellationToken);

        return result.MatchCreated(response => $"/api/posts/{response.PostId}");
    }

    [HttpPut("{postId:guid}")]
    public async Task<IResult> UpdatePost(Guid postId, [FromBody] UpdatePostRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdatePostCommand
        {
            PostId = postId,
            Content = request.Content
        };

        Result<UpdatePostCommandResponse> result = await _mediator.Send(command, cancellationToken);

        return result.MatchOk();
    }

    [HttpDelete("{postId:guid}")]
    public async Task<IResult> DeletePost(Guid postId, CancellationToken cancellationToken)
    {
        var command = new DeletePostCommand
        {
            PostId = postId
        };

        Result<DeletePostCommandResponse> result = await _mediator.Send(command, cancellationToken);

        return result.MatchOk();
    }

    [HttpPost("{postId:guid}/like")]
    public async Task<IResult> LikePost(Guid postId, CancellationToken cancellationToken)
    {
        var command = new LikePostCommand
        {
            PostId = postId
        };

        Result<LikePostCommandResponse> result = await _mediator.Send(command, cancellationToken);

        return result.MatchOk();
    }
}
