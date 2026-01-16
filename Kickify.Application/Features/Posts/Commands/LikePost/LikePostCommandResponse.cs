namespace Kickify.Application.Features.Posts.Commands.LikePost;

public class LikePostCommandResponse
{
    public Guid PostId { get; set; }
    public bool IsLiked { get; set; }
    public int TotalLikes { get; set; }
}
