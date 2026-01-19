using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.Comments.Commands.UpdateComment;

public class UpdateCommentCommand : ICommand<UpdateCommentCommandResponse>
{
    public Guid CommentId { get; set; }
    public string Content { get; set; } = string.Empty;
}
