using Kickify.Application.Abstractions.Messaging;
using Kickify.Domain.Enums;

namespace Kickify.Application.Features.MatchRooms.Commands.VoteMatchResult;

public record VoteMatchResultCommand(Guid RoomId, MatchResult Vote) : ICommand<VoteMatchResultResponse>;
