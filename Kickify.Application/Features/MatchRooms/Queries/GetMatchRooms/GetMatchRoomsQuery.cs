using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.MatchRooms.Queries.GetMatchRooms
{
    public record GetMatchRoomsQuery(
        List<DateTime>? Dates,
        string? MatchFormat,
        bool? AvailableOnly,
        decimal? Latitude,
        decimal? Longitude,
        double? RadiusKm,
        int Page = 1,
        int PageSize = 10
    ) : IQuery<GetMatchRoomsResponse>;
}
