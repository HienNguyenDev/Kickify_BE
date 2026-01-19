using Kickify.Domain.Common;
using MediatR;

namespace Kickify.Application.Features.MatchRooms.Queries.GetMatchRoomById
{
    public record GetMatchRoomByIdQuery(
        Guid RoomId
    ) : IRequest<Result<GetMatchRoomByIdResponse>>;
}
