using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.Posts.Commands.DeletePost;

public class DeletePostCommand : ICommand<DeletePostCommandResponse>
{
    public Guid PostId { get; set; }
}
