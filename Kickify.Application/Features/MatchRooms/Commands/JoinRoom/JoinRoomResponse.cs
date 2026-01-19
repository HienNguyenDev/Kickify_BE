namespace Kickify.Application.Features.MatchRooms.Commands.JoinRoom
{
    public record JoinRoomResponse(
        Guid ParticipantId,
        Guid RoomId,
        Guid UserId,
        int FilledSlots,
        int TotalSlots,
        DateTime JoinDate
    );
}
