namespace Kickify.Application.Abstractions.Services;

public interface ITrustScoreService
{
    /// <summary>
    /// Recalculate trust score for a player based on current historical data.
    /// </summary>
    Task RecalculateAsync(Guid playerId, CancellationToken cancellationToken = default);
}
