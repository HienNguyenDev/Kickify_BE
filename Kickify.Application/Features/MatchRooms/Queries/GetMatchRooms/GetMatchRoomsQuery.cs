using Kickify.Domain.Common;
using MediatR;

namespace Kickify.Application.Features.MatchRooms.Queries.GetMatchRooms
{
    public record GetMatchRoomsQuery(
        DateTime? Date,
        string? MatchFormat,
        bool? AvailableOnly,
        int Page = 1,
        int PageSize = 10
    ) : IRequest<Result<GetMatchRoomsResponse>>;
}
