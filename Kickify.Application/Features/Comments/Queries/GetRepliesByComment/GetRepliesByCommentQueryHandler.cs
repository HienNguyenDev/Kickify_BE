using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.Comments.Queries.GetRepliesByComment;

public class GetRepliesByCommentQueryHandler : IQueryHandler<GetRepliesByCommentQuery, GetRepliesByCommentQueryResponse>
{
    private readonly ICommentRepository _commentRepository;
    private readonly ICommentLikeRepository _commentLikeRepository;
    private readonly IUserContext _userContext;

    public GetRepliesByCommentQueryHandler(
        ICommentRepository commentRepository,
        ICommentLikeRepository commentLikeRepository,
        IUserContext userContext)
    {
        _commentRepository = commentRepository;
        _commentLikeRepository = commentLikeRepository;
        _userContext = userContext;
    }

    public async Task<Result<GetRepliesByCommentQueryResponse>> Handle(GetRepliesByCommentQuery request, CancellationToken cancellationToken)
    {
        var parentComment = await _commentRepository.GetByIdAsync(request.CommentId);
        if (parentComment is null || !parentComment.IsActive)
        {
            return Result.Failure<GetRepliesByCommentQueryResponse>(CommentErrors.NotFound(request.CommentId));
        }

        var (replies, total) = await _commentRepository.GetRepliesByCommentAsync(request.CommentId, request.Page, request.PageSize, cancellationToken);

        var replyList = replies.ToList();
        var replyIds = replyList.Select(r => r.CommentId).ToList();

        // Get liked reply ids by current user
        var likedReplyIds = await _commentLikeRepository.GetLikedCommentIdsByUserAsync(replyIds, _userContext.UserId, cancellationToken);

        var replyDtos = replyList.Select(r => new ReplyDto
        {
            CommentId = r.CommentId,
            UserId = r.UserId,
            UserFullName = r.User?.FullName ?? string.Empty,
            UserAvatarUrl = r.User?.AvatarUrl,
            Content = r.Content,
            TotalLikes = r.TotalLikes,
            IsEdited = r.IsEdited,
            IsLikedByCurrentUser = likedReplyIds.Contains(r.CommentId),
            CreatedAt = r.CreatedAt
        }).ToList();

        var response = new GetRepliesByCommentQueryResponse
        {
            ParentCommentId = request.CommentId,
            Replies = replyDtos,
            TotalCount = total,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling(total / (double)request.PageSize)
        };

        return Result.Success(response);
    }
}
