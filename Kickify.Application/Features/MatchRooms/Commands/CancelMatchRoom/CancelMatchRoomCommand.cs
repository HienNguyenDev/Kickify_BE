using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.MatchRooms.Commands.CancelMatchRoom;

public record CancelMatchRoomCommand(
    Guid RoomId,
    string Reason
) : ICommand<CancelMatchRoomResponse>;
