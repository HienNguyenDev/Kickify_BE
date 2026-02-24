using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.Friendships.Queries.GetFriends;

public class GetFriendsQuery : IQuery<GetFriendsQueryResponse>
{
    public string? SearchTerm { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
