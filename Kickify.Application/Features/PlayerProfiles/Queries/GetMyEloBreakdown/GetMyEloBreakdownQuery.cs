using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.PlayerProfiles.Queries.GetMyEloBreakdown;

public record GetMyEloBreakdownQuery(Guid? MatchId) : IQuery<IReadOnlyList<EloBreakdownItemResponse>>;
