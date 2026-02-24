using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.Friendships.Commands.RespondFriendRequest;

public class RespondFriendRequestCommand : ICommand<RespondFriendRequestCommandResponse>
{
    public Guid RequesterId { get; set; }
    public bool Accept { get; set; }
}
