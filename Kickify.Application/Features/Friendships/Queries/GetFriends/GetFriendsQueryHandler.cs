using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;

namespace Kickify.Application.Features.Friendships.Queries.GetFriends;

public class GetFriendsQueryHandler : IQueryHandler<GetFriendsQuery, GetFriendsQueryResponse>
{
    private readonly IFriendshipRepository _friendshipRepository;
    private readonly IUserContext _userContext;

    public GetFriendsQueryHandler(IFriendshipRepository friendshipRepository, IUserContext userContext)
    {
        _friendshipRepository = friendshipRepository;
        _userContext = userContext;
    }

    public async Task<Result<GetFriendsQueryResponse>> Handle(GetFriendsQuery request, CancellationToken cancellationToken)
    {
        var (friendships, total) = await _friendshipRepository.GetFriendsAsync(
            _userContext.UserId, 
            request.SearchTerm, 
            request.Page, 
            request.PageSize, 
            cancellationToken);

        var friendDtos = friendships.Select(f =>
        {
            var friend = f.RequesterId == _userContext.UserId ? f.Addressee : f.Requester;
            return new FriendDto
            {
                UserId = friend.UserId,
                FullName = friend.FullName ?? string.Empty,
                AvatarUrl = friend.AvatarUrl,
                CurrentElo = friend.PlayerProfile?.CurrentElo,
                PreferredPositions = friend.PlayerProfile?.PreferredPositions,
                FriendsSince = f.RespondedAt ?? f.CreatedAt
            };
        }).ToList();

        var response = new GetFriendsQueryResponse
        {
            Friends = friendDtos,
            TotalCount = total,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling(total / (double)request.PageSize)
        };

        return Result.Success(response);
    }
}
