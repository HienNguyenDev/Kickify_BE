using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.MatchRooms.Commands.LeaveRoom
{
    public record LeaveRoomCommand(
        Guid RoomId
    ) : ICommand<LeaveRoomResponse>;
}
