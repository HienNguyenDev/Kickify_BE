namespace Kickify.Application.Features.Friendships.Queries.GetPendingRequests;

public class GetPendingRequestsQueryResponse
{
    public IEnumerable<FriendRequestDto> Requests { get; set; } = new List<FriendRequestDto>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class FriendRequestDto
{
    public Guid FriendshipId { get; set; }
    public Guid RequesterId { get; set; }
    public string RequesterFullName { get; set; } = string.Empty;
    public string? RequesterAvatarUrl { get; set; }
    public DateTime SentAt { get; set; }
}
