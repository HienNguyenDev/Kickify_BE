using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.Posts.Queries.GetPostById;

public class GetPostByIdQueryHandler : IQueryHandler<GetPostByIdQuery, GetPostByIdQueryResponse>
{
    private readonly IPostRepository _postRepository;
    private readonly IPostLikeRepository _postLikeRepository;
    private readonly IUserContext _userContext;

    public GetPostByIdQueryHandler(
        IPostRepository postRepository,
        IPostLikeRepository postLikeRepository,
        IUserContext userContext)
    {
        _postRepository = postRepository;
        _postLikeRepository = postLikeRepository;
        _userContext = userContext;
    }

    public async Task<Result<GetPostByIdQueryResponse>> Handle(GetPostByIdQuery request, CancellationToken cancellationToken)
    {
        var post = await _postRepository.GetPostWithDetailsAsync(request.PostId, cancellationToken);

        if (post is null)
        {
            return Result.Failure<GetPostByIdQueryResponse>(PostErrors.NotFound(request.PostId));
        }

        var isLikedByCurrentUser = await _postLikeRepository.IsPostLikedByUserAsync(post.PostId, _userContext.UserId, cancellationToken);

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
            IsLikedByCurrentUser = isLikedByCurrentUser,
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
