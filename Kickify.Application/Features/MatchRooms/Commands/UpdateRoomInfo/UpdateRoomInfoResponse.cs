namespace Kickify.Application.Features.MatchRooms.Commands.UpdateRoomInfo
{
    public record UpdateRoomInfoResponse(
        Guid RoomId,
        string? RoomName,
        string? Description,
        string? Rules,
        DateTime UpdatedAt
    );
}
