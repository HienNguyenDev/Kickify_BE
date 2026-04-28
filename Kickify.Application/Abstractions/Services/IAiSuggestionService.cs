namespace Kickify.Application.Abstractions.Services;

public interface IAiSuggestionService
{
    /// <summary>
    /// Parse a Vietnamese natural-language query into structured match-room search filters.
    /// Returns null only on hard service failure; check IsRelevant on the result otherwise.
    /// </summary>
    Task<RoomQueryParseResult?> ParseRoomQueryAsync(
        RoomQueryParseRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Parse a Vietnamese natural-language query into structured player search filters,
    /// optionally returning high-form player IDs from the ChromaDB vector store.
    /// </summary>
    Task<PlayerQueryParseResult?> ParsePlayerQueryAsync(
        PlayerQueryParseRequest request,
        CancellationToken cancellationToken = default);
}

// ─── Request models ────────────────────────────────────────────────────────────

public record RoomQueryParseRequest(
    string Query,
    string CurrentDate,
    string CurrentTime,
    double? UserLatitude = null,
    double? UserLongitude = null);

public record PlayerQueryParseRequest(
    string Query,
    string CurrentDate,
    List<string>? ExcludePlayerIds = null);

// ─── Result models ──────────────────────────────────────────────────────────────

public record RoomQueryParseResult(
    bool IsRelevant,
    string? MatchFormat,
    DateTime? Date,
    TimeSpan? TimeFrom,
    TimeSpan? TimeTo,
    string? LocationName,
    bool AvailableOnly = true);

public record PlayerQueryParseResult(
    bool IsRelevant,
    List<string>? Positions,
    int? MinElo,
    int? MaxElo,
    string? PreferredFoot,
    bool HighFormOnly,
    decimal? MinTrustScore,
    List<string> SimilarPlayerIds);
