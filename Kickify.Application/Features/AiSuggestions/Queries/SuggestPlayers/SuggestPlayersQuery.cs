using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Features.PlayerProfiles.Queries.GetAllPlayerProfiles;

namespace Kickify.Application.Features.AiSuggestions.Queries.SuggestPlayers;

public record SuggestPlayersQuery(
    string Query,
    Guid? RoomId,
    int Page = 1,
    int PageSize = 10
) : IQuery<GetAllPlayerProfilesQueryResponse>;
