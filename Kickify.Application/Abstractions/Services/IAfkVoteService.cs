namespace Kickify.Application.Abstractions.Services;

public interface IAfkVoteService
{
    /// <summary>
    /// Recalculate AFK vote counts and confirmation flags for all participants in a match.
    /// </summary>
    Task RecalculateMatchAfkStatusAsync(Guid matchRoomId, CancellationToken cancellationToken = default);
}
