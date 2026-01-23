using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.Friendships.Commands.SendFriendRequest;

public class SendFriendRequestCommand : ICommand<SendFriendRequestCommandResponse>
{
    public Guid AddresseeId { get; set; }
}
