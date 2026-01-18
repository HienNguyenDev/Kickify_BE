namespace Kickify.Application.Features.MatchRooms.Commands.LeaveRoom
{
    public record LeaveRoomResponse(
        Guid RoomId,
        Guid UserId,
        int FilledSlots,
        int TotalSlots,
        string Message
    );
}
