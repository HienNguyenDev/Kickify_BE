using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.Friendships.Queries.GetFriendshipStatus;

public class GetFriendshipStatusQuery : IQuery<GetFriendshipStatusQueryResponse>
{
    public Guid OtherUserId { get; set; }
}
