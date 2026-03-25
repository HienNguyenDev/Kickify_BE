using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.MatchRooms.Commands.UpdateRoomInfo
{
    public record UpdateRoomInfoCommand(
        Guid RoomId,
        string? RoomName,
        string? Description,
        string? Rules
    ) : ICommand<UpdateRoomInfoResponse>;
}
