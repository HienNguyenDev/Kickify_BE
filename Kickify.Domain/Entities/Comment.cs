using Kickify.Domain.Common;

namespace Kickify.Domain.Entities;

public class Comment : BaseEntity
{
    public Guid CommentId { get; set; }
    public Guid PostId { get; set; }
    public Guid UserId { get; set; }
    public Guid? ParentCommentId { get; set; } // NULL = root comment, NOT NULL = reply
    public string Content { get; set; } = string.Empty;
    public int TotalLikes { get; set; } = 0;
    public int TotalReplies { get; set; } = 0; // Only count for root comments
    public bool IsEdited { get; set; } = false;
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public Post Post { get; set; } = null!;
    public User User { get; set; } = null!;
    public Comment? ParentComment { get; set; }
    public ICollection<Comment> Replies { get; set; } = new List<Comment>();
    public ICollection<CommentLike> CommentLikes { get; set; } = new List<CommentLike>();
}
