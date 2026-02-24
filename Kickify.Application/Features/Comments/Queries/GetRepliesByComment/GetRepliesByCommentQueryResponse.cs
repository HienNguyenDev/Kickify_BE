namespace Kickify.Application.Features.Comments.Queries.GetRepliesByComment;

public class GetRepliesByCommentQueryResponse
{
    public Guid ParentCommentId { get; set; }
    public IEnumerable<ReplyDto> Replies { get; set; } = new List<ReplyDto>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class ReplyDto
{
    public Guid CommentId { get; set; }
    public Guid UserId { get; set; }
    public string UserFullName { get; set; } = string.Empty;
    public string? UserAvatarUrl { get; set; }
    public string Content { get; set; } = string.Empty;
    public int TotalLikes { get; set; }
    public bool IsEdited { get; set; }
    public bool IsLikedByCurrentUser { get; set; }
    public DateTime CreatedAt { get; set; }
}
