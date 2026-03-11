using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Entities;
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
            }).ToList(),
            LikedByUsers = post.PostLikes
                .OrderByDescending(pl => pl.CreatedAt)
                .Select(pl => new PostLikeUserDto
                {
                    UserId = pl.UserId,
                    FullName = pl.User?.FullName,
                    AvatarUrl = pl.User?.AvatarUrl,
                    LikedAt = pl.CreatedAt
                }).ToList(),
            Comments = BuildCommentTree(post.Comments)
        };

        return Result.Success(response);
    }

    private static List<PostCommentDto> BuildCommentTree(ICollection<Comment> allComments)
    {
        var lookup = allComments.ToLookup(c => c.ParentCommentId);

        return lookup[null]
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => MapComment(c, lookup))
            .ToList();
    }

    private static PostCommentDto MapComment(Comment comment, ILookup<Guid?, Comment> lookup)
    {
        var replies = lookup[comment.CommentId]
            .OrderBy(r => r.CreatedAt)
            .Select(r => MapComment(r, lookup))
            .ToList();

        return new PostCommentDto
        {
            CommentId = comment.CommentId,
            UserId = comment.UserId,
            UserFullName = comment.User?.FullName,
            UserAvatarUrl = comment.User?.AvatarUrl,
            Content = comment.Content,
            TotalLikes = comment.TotalLikes,
            TotalReplies = CountDescendants(comment.CommentId, lookup),
            IsEdited = comment.IsEdited,
            CreatedAt = comment.CreatedAt,
            Replies = replies
        };
    }

    private static int CountDescendants(Guid commentId, ILookup<Guid?, Comment> lookup)
    {
        var children = lookup[commentId].ToList();
        var count = children.Count;
        foreach (var child in children)
        {
            count += CountDescendants(child.CommentId, lookup);
        }
        return count;
    }
}
