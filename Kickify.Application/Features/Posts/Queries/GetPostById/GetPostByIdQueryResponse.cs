using Kickify.Domain.Enums;

namespace Kickify.Application.Features.Posts.Queries.GetPostById;

public class GetPostByIdQueryResponse
{
    public Guid PostId { get; set; }
    public Guid UserId { get; set; }
    public string UserFullName { get; set; } = string.Empty;
    public string? UserAvatarUrl { get; set; }
    public string Content { get; set; } = string.Empty;
    public int TotalMedia { get; set; }
    public int TotalLikes { get; set; }
    public int TotalComments { get; set; }
    public PostVisibility Visibility { get; set; }
    public bool IsEdited { get; set; }
    public DateTime? EditedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsLikedByCurrentUser { get; set; }
    public List<PostMediaDto> Media { get; set; } = new();
    public List<PostLikeUserDto> LikedByUsers { get; set; } = new();
    public List<PostCommentDto> Comments { get; set; } = new();
}

public class PostMediaDto
{
    public Guid MediaId { get; set; }
    public string PublicUrl { get; set; } = string.Empty;
    public MediaType MediaType { get; set; }
    public int DisplayOrder { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public int? Duration { get; set; }
}

public class PostLikeUserDto
{
    public Guid UserId { get; set; }
    public string? FullName { get; set; }
    public string? AvatarUrl { get; set; }
    public DateTime LikedAt { get; set; }
}

public class PostCommentDto
{
    public Guid CommentId { get; set; }
    public Guid UserId { get; set; }
    public string? UserFullName { get; set; }
    public string? UserAvatarUrl { get; set; }
    public string Content { get; set; } = string.Empty;
    public int TotalLikes { get; set; }
    public int TotalReplies { get; set; }
    public DateTime CreatedAt { get; set; }
}
