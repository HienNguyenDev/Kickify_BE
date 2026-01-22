namespace Kickify.Application.Features.Comments.Commands.LikeComment;

public class LikeCommentCommandResponse
{
    public Guid CommentId { get; set; }
    public bool IsLiked { get; set; }
    public int TotalLikes { get; set; }
}
