using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.Comments.Commands.LikeComment;

public class LikeCommentCommand : ICommand<LikeCommentCommandResponse>
{
    public Guid CommentId { get; set; }
}
