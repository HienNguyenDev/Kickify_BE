using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.MatchRooms.Queries.GetMatchRoomById
{
    public record GetMatchRoomByIdQuery(
        Guid RoomId
    ) : IQuery<GetMatchRoomByIdResponse>;
}
