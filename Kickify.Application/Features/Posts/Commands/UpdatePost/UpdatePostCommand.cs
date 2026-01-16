using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.Posts.Commands.UpdatePost;

public class UpdatePostCommand : ICommand<UpdatePostCommandResponse>
{
    public Guid PostId { get; set; }
    public string Content { get; set; } = string.Empty;
}
