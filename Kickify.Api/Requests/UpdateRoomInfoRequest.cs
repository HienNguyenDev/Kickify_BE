namespace Kickify.Api.Requests
{
    public record UpdateRoomInfoRequest(
        string? RoomName,
        string? Description,
        string? Rules
    );
}
