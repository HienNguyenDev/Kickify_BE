namespace Kickify.Application.Features.Comments.Commands.UpdateComment;

public class UpdateCommentCommandResponse
{
    public Guid CommentId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}
