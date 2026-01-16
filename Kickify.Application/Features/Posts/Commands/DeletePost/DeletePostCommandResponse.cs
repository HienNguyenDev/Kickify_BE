namespace Kickify.Application.Features.Posts.Commands.DeletePost;

public class DeletePostCommandResponse
{
    public Guid PostId { get; set; }
    public DateTime DeletedAt { get; set; }
}
