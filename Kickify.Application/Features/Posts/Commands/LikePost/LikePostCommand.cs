using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.Posts.Commands.LikePost;

public class LikePostCommand : ICommand<LikePostCommandResponse>
{
    public Guid PostId { get; set; }
}
