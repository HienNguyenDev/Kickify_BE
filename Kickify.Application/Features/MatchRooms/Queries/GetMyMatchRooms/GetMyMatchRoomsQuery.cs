using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.MatchRooms.Queries.GetMyMatchRooms
{
    public record GetMyMatchRoomsQuery(
        int Page = 1,
        int PageSize = 10
    ) : IQuery<GetMyMatchRoomsResponse>;
}
