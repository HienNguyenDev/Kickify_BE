using Kickify.Domain.Common;
using MediatR;

namespace Kickify.Application.Features.MatchRooms.Commands.LeaveRoom
{
    public record LeaveRoomCommand(
        Guid UserId,
        Guid RoomId
    ) : IRequest<Result<LeaveRoomResponse>>;
}
