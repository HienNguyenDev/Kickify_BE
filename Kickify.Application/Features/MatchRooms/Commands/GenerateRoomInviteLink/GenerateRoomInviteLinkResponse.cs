namespace Kickify.Application.Features.MatchRooms.Commands.GenerateRoomInviteLink;

public record GenerateRoomInviteLinkResponse(
    Guid InvitationId,
    Guid RoomId,
    string DeepLink,
    string WebLink,
    string QrCodeUrl,
    DateTime CreatedAt
);
