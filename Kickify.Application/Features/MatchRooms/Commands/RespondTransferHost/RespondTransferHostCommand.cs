using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.MatchRooms.Commands.RespondTransferHost;

public record RespondTransferHostCommand(
    Guid RoomId,
    bool IsAccepted
) : ICommand<RespondTransferHostResponse>;
