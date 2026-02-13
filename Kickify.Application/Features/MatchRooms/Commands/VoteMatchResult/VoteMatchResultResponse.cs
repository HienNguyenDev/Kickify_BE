using Kickify.Domain.Enums;

namespace Kickify.Application.Features.MatchRooms.Commands.VoteMatchResult;

public record VoteMatchResultResponse(
    Guid RoomId,
    Guid UserId,
    MatchResult Vote,
    DateTime VotedAt,
    int VoteCount,
    int TotalParticipants,
    double VotePercentage,
    bool ThresholdReached,
    string Message);
