namespace Kickify.Application.Features.MatchRooms.Commands.UpdateRoomPrivacy
{
    public record UpdateRoomPrivacyResponse(
        Guid RoomId,
        string Visibility,
        bool IsPrivate,
        DateTime UpdatedAt
    );
}
