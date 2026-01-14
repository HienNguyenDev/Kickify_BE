using Kickify.Api.Extensions;
using Kickify.Api.Requests;
using Kickify.Application.Abstractions.Services;
using Kickify.Application.Features.Posts.Commands.CreatePost;
using Kickify.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kickify.Api.Controllers
{
    [Route("api/")]
    [ApiController]
    [Authorize]
    public class PostsController : ControllerBase
    {
        private readonly ISender _mediator;

        public PostsController(ISender mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("posts")]
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
    }
}
