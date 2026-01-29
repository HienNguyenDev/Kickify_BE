using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.MatchPresets.Queries.GetMatchPresets
{
    public record GetMatchPresetsQuery(
        int Page = 1,
        int PageSize = 10
    ) : IQuery<GetMatchPresetsResponse>;
}
