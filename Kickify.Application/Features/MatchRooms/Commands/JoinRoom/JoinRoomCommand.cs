using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.MatchRooms.Commands.JoinRoom
{
    public record JoinRoomCommand(
        Guid RoomId,
        string? Password
    ) : ICommand<JoinRoomResponse>;
}
