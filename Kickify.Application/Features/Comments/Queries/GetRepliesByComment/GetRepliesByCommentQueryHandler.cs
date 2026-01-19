using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.Comments.Queries.GetRepliesByComment;

public class GetRepliesByCommentQueryHandler : IQueryHandler<GetRepliesByCommentQuery, GetRepliesByCommentQueryResponse>
{
    private readonly ICommentRepository _commentRepository;

    public GetRepliesByCommentQueryHandler(ICommentRepository commentRepository)
    {
        _commentRepository = commentRepository;
    }

    public async Task<Result<GetRepliesByCommentQueryResponse>> Handle(GetRepliesByCommentQuery request, CancellationToken cancellationToken)
    {
        var parentComment = await _commentRepository.GetByIdAsync(request.CommentId);
        if (parentComment is null || !parentComment.IsActive)
        {
            return Result.Failure<GetRepliesByCommentQueryResponse>(CommentErrors.NotFound(request.CommentId));
        }
        var (replies, total) = await _commentRepository.GetRepliesByCommentAsync(request.CommentId, request.Page, request.PageSize, cancellationToken);

        var replyDtos = replies.Select(r => new ReplyDto
        {
            CommentId = r.CommentId,
            UserId = r.UserId,
            UserFullName = r.User?.FullName ?? string.Empty,
            UserAvatarUrl = r.User?.AvatarUrl,
            Content = r.Content,
            TotalLikes = r.TotalLikes,
            IsEdited = r.IsEdited,
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
