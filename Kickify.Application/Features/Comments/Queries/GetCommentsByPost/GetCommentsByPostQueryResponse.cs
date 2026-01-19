namespace Kickify.Application.Features.Comments.Queries.GetCommentsByPost;

public class GetCommentsByPostQueryResponse
{
    public IEnumerable<CommentDto> Comments { get; set; } = new List<CommentDto>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class CommentDto
{
    public Guid CommentId { get; set; }
    public Guid UserId { get; set; }
    public string UserFullName { get; set; } = string.Empty;
    public string? UserAvatarUrl { get; set; }
    public string Content { get; set; } = string.Empty;
    public int TotalLikes { get; set; }
    public int TotalReplies { get; set; }
    public bool IsEdited { get; set; }
    public DateTime CreatedAt { get; set; }
}
