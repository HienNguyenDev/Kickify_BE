using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Domain.Common;
using Kickify.Domain.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kickify.Application.Features.MatchRooms.Queries.GetAfkVoteStatus;

public class GetAfkVoteStatusQueryHandler : IQueryHandler<GetAfkVoteStatusQuery, IReadOnlyList<AfkVoteStatusItemResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserContext _userContext;

    public GetAfkVoteStatusQueryHandler(IApplicationDbContext dbContext, IUserContext userContext)
    {
        _dbContext = dbContext;
        _userContext = userContext;
    }

    /// <summary>
    /// Get current AFK vote status for all participants in a room.
    /// </summary>
    public async Task<Result<IReadOnlyList<AfkVoteStatusItemResponse>>> Handle(GetAfkVoteStatusQuery request, CancellationToken cancellationToken)
    {
        var userId = _userContext.UserId;
        var isParticipant = await _dbContext.RoomParticipants
            .AnyAsync(x => x.RoomId == request.MatchRoomId && x.UserId == userId, cancellationToken);

        if (!isParticipant)
        {
            return Result.Failure<IReadOnlyList<AfkVoteStatusItemResponse>>(MatchRoomErrors.NotParticipant);
        }

        var players = await _dbContext.RoomParticipants
            .Where(x => x.RoomId == request.MatchRoomId)
            .Select(x => new AfkVoteStatusItemResponse(
                x.UserId,
                x.TeamAssignment,
                x.AfkVoteCount,
                x.IsConfirmedAfk))
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<AfkVoteStatusItemResponse>>(players);
    }
}
