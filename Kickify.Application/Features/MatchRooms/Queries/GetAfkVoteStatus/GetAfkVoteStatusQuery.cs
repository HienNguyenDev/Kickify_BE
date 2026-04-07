using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.MatchRooms.Queries.GetAfkVoteStatus;

public record GetAfkVoteStatusQuery(Guid MatchRoomId) : IQuery<IReadOnlyList<AfkVoteStatusItemResponse>>;
