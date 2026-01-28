using Kickify.Domain.Enums;

namespace Kickify.Application.Features.Posts.Queries.GetAllPosts;

public class GetAllPostsQueryResponse
{
    public IEnumerable<PostDto> Posts { get; set; } = new List<PostDto>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class PostDto
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
    public bool IsLikedByCurrentUser { get; set; }
    public List<PostMediaDto> Media { get; set; } = new();
}

public class PostMediaDto
{
    public Guid MediaId { get; set; }
    public string PublicUrl { get; set; } = string.Empty;
    public MediaType MediaType { get; set; }
    public int DisplayOrder { get; set; }
}
