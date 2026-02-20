namespace Kickify.Application.Features.MatchRooms.Commands.InviteFriendToRoom;

public record InviteFriendToRoomResponse(
    Guid InvitationId,
    Guid RoomId,
    Guid InviterId,
    Guid InviteeId,
    string DeepLink,
    string Status,
    DateTime CreatedAt
);
