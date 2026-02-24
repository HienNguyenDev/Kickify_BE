namespace Kickify.Application.Features.Friendships.Commands.RemoveFriend;

public class RemoveFriendCommandResponse
{
    public Guid FriendId { get; set; }
    public DateTime RemovedAt { get; set; }
}
