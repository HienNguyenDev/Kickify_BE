namespace Kickify.Application.Features.Comments.Commands.CreateComment;

public class CreateCommentCommandResponse
{
    public Guid CommentId { get; set; }
    public Guid PostId { get; set; }
    public Guid? ParentCommentId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
