using Kickify.Domain.Common;
using MediatR;

namespace Kickify.Application.Features.MatchRooms.Commands.JoinRoom
{
    public record JoinRoomCommand(
        Guid UserId,
        Guid RoomId
    ) : IRequest<Result<JoinRoomResponse>>;
}
