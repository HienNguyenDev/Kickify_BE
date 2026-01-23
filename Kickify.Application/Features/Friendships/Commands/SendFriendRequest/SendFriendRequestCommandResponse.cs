namespace Kickify.Application.Features.Friendships.Commands.SendFriendRequest;

public class SendFriendRequestCommandResponse
{
    public Guid FriendshipId { get; set; }
    public Guid RequesterId { get; set; }
    public Guid AddresseeId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
