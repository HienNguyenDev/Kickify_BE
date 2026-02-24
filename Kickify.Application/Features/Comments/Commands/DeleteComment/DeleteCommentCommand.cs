using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.Comments.Commands.DeleteComment;

public class DeleteCommentCommand : ICommand<DeleteCommentCommandResponse>
{
    public Guid CommentId { get; set; }
}
