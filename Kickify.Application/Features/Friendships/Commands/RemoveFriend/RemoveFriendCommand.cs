using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.Friendships.Commands.RemoveFriend;

public class RemoveFriendCommand : ICommand<RemoveFriendCommandResponse>
{
    public Guid FriendId { get; set; }
}
