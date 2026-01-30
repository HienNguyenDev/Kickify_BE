using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.Comments.Queries.GetCommentsByPost;

public class GetCommentsByPostQueryHandler : IQueryHandler<GetCommentsByPostQuery, GetCommentsByPostQueryResponse>
{
    private readonly ICommentRepository _commentRepository;
    private readonly ICommentLikeRepository _commentLikeRepository;
    private readonly IPostRepository _postRepository;
    private readonly IUserContext _userContext;

    public GetCommentsByPostQueryHandler(
        ICommentRepository commentRepository,
        ICommentLikeRepository commentLikeRepository,
        IPostRepository postRepository,
        IUserContext userContext)
    {
        _commentRepository = commentRepository;
        _commentLikeRepository = commentLikeRepository;
        _postRepository = postRepository;
        _userContext = userContext;
    }

    public async Task<Result<GetCommentsByPostQueryResponse>> Handle(GetCommentsByPostQuery request, CancellationToken cancellationToken)
    {
        var post = await _postRepository.GetByIdAsync(request.PostId);
        if (post is null)
        {
            return Result.Failure<GetCommentsByPostQueryResponse>(CommentErrors.PostNotFound);
        }      

        var (comments, total) = await _commentRepository.GetCommentsByPostAsync(request.PostId, request.Page, request.PageSize, cancellationToken);

        var commentList = comments.ToList();
        var commentIds = commentList.Select(c => c.CommentId).ToList();

        // Get liked comment ids by current user
        var likedCommentIds = await _commentLikeRepository.GetLikedCommentIdsByUserAsync(commentIds, _userContext.UserId, cancellationToken);

        var commentDtos = commentList.Select(c => new CommentDto
        {
            CommentId = c.CommentId,
            UserId = c.UserId,
            UserFullName = c.User?.FullName ?? string.Empty,
            UserAvatarUrl = c.User?.AvatarUrl,
            Content = c.Content,
            TotalLikes = c.TotalLikes,
            TotalReplies = c.TotalReplies,
            IsEdited = c.IsEdited,
            IsLikedByCurrentUser = likedCommentIds.Contains(c.CommentId),
            CreatedAt = c.CreatedAt
        }).ToList();

        var response = new GetCommentsByPostQueryResponse
        {
            Comments = commentDtos,
            TotalCount = total,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling(total / (double)request.PageSize)
        };

        return Result.Success(response);
    }
}
