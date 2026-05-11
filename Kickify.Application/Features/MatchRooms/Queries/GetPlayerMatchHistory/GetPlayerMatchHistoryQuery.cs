using System;
using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.MatchRooms.Queries.GetPlayerMatchHistory
{
    public record GetPlayerMatchHistoryQuery(
        Guid TargetUserId,
        int Page = 1,
        int PageSize = 10
    ) : IQuery<GetPlayerMatchHistoryResponse>;
}
