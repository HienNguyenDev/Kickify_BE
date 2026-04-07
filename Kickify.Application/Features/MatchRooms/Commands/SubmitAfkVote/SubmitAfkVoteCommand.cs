using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.MatchRooms.Commands.SubmitAfkVote;

public record SubmitAfkVoteCommand(Guid MatchRoomId, IReadOnlyCollection<Guid> TargetPlayerIds) : ICommand<SubmitAfkVoteResponse>;
