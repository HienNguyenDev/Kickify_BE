using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Features.MatchRooms.Queries.GetMatchRooms;

namespace Kickify.Application.Features.AiSuggestions.Queries.SuggestMatchRooms;

public record SuggestMatchRoomsQuery(
    string Query,
    double? Latitude,
    double? Longitude,
    int Page = 1,
    int PageSize = 10
) : IQuery<GetMatchRoomsResponse>;
