using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.Comments.Queries.GetCommentsByPost;

public class GetCommentsByPostQueryHandler : IQueryHandler<GetCommentsByPostQuery, GetCommentsByPostQueryResponse>
{
    private readonly ICommentRepository _commentRepository;
    private readonly IPostRepository _postRepository;

    public GetCommentsByPostQueryHandler(ICommentRepository commentRepository, IPostRepository postRepository)
    {
        _commentRepository = commentRepository;
        _postRepository = postRepository;
    }

    public async Task<Result<GetCommentsByPostQueryResponse>> Handle(GetCommentsByPostQuery request, CancellationToken cancellationToken)
    {
        var post = await _postRepository.GetByIdAsync(request.PostId);
        if (post is null)
        {
            return Result.Failure<GetCommentsByPostQueryResponse>(CommentErrors.PostNotFound);
        }      

        var (comments, total) = await _commentRepository.GetCommentsByPostAsync(request.PostId, request.Page, request.PageSize, cancellationToken);

        var commentDtos = comments.Select(c => new CommentDto
        {
            CommentId = c.CommentId,
            UserId = c.UserId,
            UserFullName = c.User?.FullName ?? string.Empty,
            UserAvatarUrl = c.User?.AvatarUrl,
            Content = c.Content,
            TotalLikes = c.TotalLikes,
            TotalReplies = c.TotalReplies,
            IsEdited = c.IsEdited,
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
