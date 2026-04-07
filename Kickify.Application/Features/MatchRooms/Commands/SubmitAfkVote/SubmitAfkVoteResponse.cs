namespace Kickify.Application.Features.MatchRooms.Commands.SubmitAfkVote;

public record SubmitAfkVoteResponse(
    Guid MatchRoomId,
    Guid VoterId,
    int CreatedVotes,
    string Message
);
