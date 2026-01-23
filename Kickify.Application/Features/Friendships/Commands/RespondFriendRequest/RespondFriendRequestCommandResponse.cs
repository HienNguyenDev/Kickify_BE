namespace Kickify.Application.Features.Friendships.Commands.RespondFriendRequest;

public class RespondFriendRequestCommandResponse
{
    public Guid FriendshipId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime RespondedAt { get; set; }
}
