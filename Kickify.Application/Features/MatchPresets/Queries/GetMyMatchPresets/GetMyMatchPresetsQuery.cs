using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.MatchPresets.Queries.GetMyMatchPresets
{
    public record GetMyMatchPresetsQuery(
        Guid UserId,
        int Page = 1,
        int PageSize = 10
    ) : IQuery<GetMyMatchPresetsResponse>;
}
