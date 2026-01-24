namespace Kickify.Application.Features.Friendships.Queries.GetFriendshipStatus;

public class GetFriendshipStatusQueryResponse
{
    public Guid OtherUserId { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid? FriendshipId { get; set; }
}
