using Kickify.Domain.Enums;

namespace Kickify.Application.Features.MatchRooms.Queries.GetAfkVoteStatus;

public record AfkVoteStatusItemResponse(
    Guid PlayerId,
    TeamAssignment Team,
    int AfkVoteCount,
    bool IsConfirmedAfk
);
