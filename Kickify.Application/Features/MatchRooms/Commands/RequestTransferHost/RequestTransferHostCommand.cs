using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.MatchRooms.Commands.RequestTransferHost;

public record RequestTransferHostCommand(
    Guid RoomId,
    Guid TargetUserId
) : ICommand<RequestTransferHostResponse>;
