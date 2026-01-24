using Kickify.Domain.Common;

namespace Kickify.Domain.Errors;

public static class FriendshipErrors
{
    public static Error NotFound(Guid friendshipId) => Error.NotFound("Friendships.NotFound", $"The friendship with the Id = '{friendshipId}' was not found");
    public static readonly Error UserNotFound = Error.NotFound("Friendships.UserNotFound", "The user was not found");
    public static readonly Error CannotAddSelf = Error.Problem("Friendships.CannotAddSelf", "You cannot send a friend request to yourself");
    public static readonly Error AlreadyFriends = Error.Conflict("Friendships.AlreadyFriends", "You are already friends with this user");
    public static readonly Error RequestAlreadyExists = Error.Conflict("Friendships.RequestAlreadyExists", "A friend request already exists between you and this user");
    public static readonly Error RequestNotFound = Error.NotFound("Friendships.RequestNotFound", "Friend request not found");
    public static readonly Error Unauthorized = Error.Problem("Friendships.Unauthorized", "You are not authorized to perform this action");
    public static readonly Error NotFriends = Error.Conflict("Friendships.NotFriends", "You are not friends with this user");
    public static readonly Error CannotChat = Error.Conflict("Friendships.CannotChat", "You must be friends to chat with this user");
}
