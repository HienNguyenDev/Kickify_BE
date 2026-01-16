using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.Posts.Queries.GetPostById;

public class GetPostByIdQueryHandler : IQueryHandler<GetPostByIdQuery, GetPostByIdQueryResponse>
{
    private readonly IPostRepository _postRepository;

    public GetPostByIdQueryHandler(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<Result<GetPostByIdQueryResponse>> Handle(GetPostByIdQuery request, CancellationToken cancellationToken)
    {
        var post = await _postRepository.GetPostWithDetailsAsync(request.PostId, cancellationToken);

        if (post is null)
        {
            return Result.Failure<GetPostByIdQueryResponse>(PostErrors.NotFound(request.PostId));
        }

        var response = new GetPostByIdQueryResponse
        {
            PostId = post.PostId,
            UserId = post.UserId,
            UserFullName = post.User?.FullName ?? string.Empty,
            UserAvatarUrl = post.User?.AvatarUrl,
            Content = post.Content,
            TotalMedia = post.TotalMedia,
            TotalLikes = post.TotalLikes,
            TotalComments = post.TotalComments,
            Visibility = post.Visibility,
            IsEdited = post.IsEdited,
            EditedAt = post.EditedAt,
            CreatedAt = post.CreatedAt,
            UpdatedAt = post.UpdatedAt,
            Media = post.PostMedia.Select(m => new PostMediaDto
            {
                MediaId = m.MediaId,
                PublicUrl = m.PublicUrl,
                MediaType = m.MediaType,
                DisplayOrder = m.DisplayOrder,
                Width = m.Width,
                Height = m.Height,
                Duration = m.Duration
            }).ToList()
        };

        return Result.Success(response);
    }
}
