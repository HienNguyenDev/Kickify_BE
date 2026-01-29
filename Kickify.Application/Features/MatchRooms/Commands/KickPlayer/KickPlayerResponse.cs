namespace Kickify.Application.Features.MatchRooms.Commands.KickPlayer
{
    public record KickPlayerResponse(
        Guid RoomId,
        Guid KickedUserId,
        string KickedUserName,
        int FilledSlots,
        int TotalSlots,
        string RoomStatus,
        string Message
    );
}
