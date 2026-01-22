using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.Comments.Commands.CreateComment;

public class CreateCommentCommand : ICommand<CreateCommentCommandResponse>
{
    public Guid PostId { get; set; }
    public Guid? ParentCommentId { get; set; }
    public string Content { get; set; } = string.Empty;
}
