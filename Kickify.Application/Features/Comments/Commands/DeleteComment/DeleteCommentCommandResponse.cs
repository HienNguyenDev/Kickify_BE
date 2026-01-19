namespace Kickify.Application.Features.Comments.Commands.DeleteComment;

public class DeleteCommentCommandResponse
{
    public Guid CommentId { get; set; }
    public DateTime DeletedAt { get; set; }
}
