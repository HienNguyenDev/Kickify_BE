using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.Friendships.Queries.GetFriendshipStatus;

public class GetFriendshipStatusQueryHandler : IQueryHandler<GetFriendshipStatusQuery, GetFriendshipStatusQueryResponse>
{
    private readonly IFriendshipRepository _friendshipRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserContext _userContext;

    public GetFriendshipStatusQueryHandler(IFriendshipRepository friendshipRepository, IUserRepository userRepository, IUserContext userContext)
    {
        _friendshipRepository = friendshipRepository;
        _userRepository = userRepository;
        _userContext = userContext;
    }

    public async Task<Result<GetFriendshipStatusQueryResponse>> Handle(GetFriendshipStatusQuery request, CancellationToken cancellationToken)
    {
        if (request.OtherUserId == _userContext.UserId) return Result.Failure<GetFriendshipStatusQueryResponse>(FriendshipErrors.CannotAddSelf);

        var otherUser = await _userRepository.GetByIdAsync(request.OtherUserId);
        if (otherUser is null) return Result.Failure<GetFriendshipStatusQueryResponse>(FriendshipErrors.UserNotFound);

        var friendship = await _friendshipRepository.GetFriendshipAsync(_userContext.UserId, request.OtherUserId, cancellationToken);

        var response = new GetFriendshipStatusQueryResponse
        {
            OtherUserId = request.OtherUserId,
            FriendshipId = friendship?.FriendshipId
        };

        if (friendship is null)
        {
            response.Status = RelationshipStatus.None.ToString();
        }
        else if (friendship.Status == FriendshipStatus.Accepted)
        {
            response.Status = RelationshipStatus.Friends.ToString();
        }
        else if (friendship.Status == FriendshipStatus.Pending)
        {
            response.Status = friendship.RequesterId == _userContext.UserId 
                ? RelationshipStatus.PendingSentByMe.ToString() 
                : RelationshipStatus.PendingReceivedFromThem.ToString();
        }
        else
        {
            response.Status = RelationshipStatus.None.ToString();
        }

        return Result.Success(response);
    }
}
