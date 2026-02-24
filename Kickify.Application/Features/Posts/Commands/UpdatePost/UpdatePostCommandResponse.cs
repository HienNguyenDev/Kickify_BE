namespace Kickify.Application.Features.Posts.Commands.UpdatePost;

public class UpdatePostCommandResponse
{
    public Guid PostId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}
