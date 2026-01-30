using Kickify.Domain.Common;

namespace Kickify.Domain.Entities;

public class CommentLike
{
    public Guid LikeId { get; set; }
    public Guid CommentId { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public Comment Comment { get; set; } = null!;
    public User User { get; set; } = null!;
}
