namespace Kickify.Application.Features.Friendships.Queries.GetFriends;

public class GetFriendsQueryResponse
{
    public IEnumerable<FriendDto> Friends { get; set; } = new List<FriendDto>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class FriendDto
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public int? CurrentElo { get; set; }
    public string? PreferredPositions { get; set; }
    public DateTime FriendsSince { get; set; }
}
