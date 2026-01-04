using Kickify.Domain.Common;
using Kickify.Domain.Enums;

namespace Kickify.Domain.Entities;

public class Post : BaseEntity
{
    public Guid PostId { get; set; }
    public Guid UserId { get; set; }
    public string Content { get; set; } = string.Empty;
    public int TotalMedia { get; set; } = 0;
    public int TotalLikes { get; set; } = 0;
    public int TotalComments { get; set; } = 0;
    public PostVisibility Visibility { get; set; } = PostVisibility.Public;
    public bool IsEdited { get; set; } = false;
    public DateTime? EditedAt { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public User User { get; set; } = null!;
    public ICollection<PostMedia> PostMedia { get; set; } = new List<PostMedia>();
    public ICollection<PostLike> PostLikes { get; set; } = new List<PostLike>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
