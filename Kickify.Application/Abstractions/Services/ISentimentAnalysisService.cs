namespace Kickify.Application.Abstractions.Services;

public interface ISentimentAnalysisService
{
    /// <summary>
    /// Send a batch of feedbacks for a specific player in a match to the AI sentiment analysis API.
    /// </summary>
    Task<SentimentBatchResponse?> SendFeedbacksForAnalysisAsync(
        SentimentBatchRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Request AI service to calculate ELO delta with full component breakdown.
    /// </summary>
    Task<EloCalculationResponse?> CalculateEloAsync(
        EloCalculationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Request AI service to generate radar chart metrics and personalized assessment.
    /// </summary>
    Task<RadarAnalysisResponse?> AnalyzeRadarAsync(
        RadarAnalysisRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Request AI service to generate feedback suggestion lines by star rating and role.
    /// </summary>
    Task<FeedbackSuggestionResponse?> GenerateFeedbackSuggestionsAsync(
        FeedbackSuggestionRequest request,
        CancellationToken cancellationToken = default);
}

public record SentimentBatchRequest(
    string MatchId,
    string TargetPlayerId,
    List<SentimentFeedbackItem> Feedbacks);

public record SentimentFeedbackItem(
    string FeedbackId,
    string ReviewerId,
    string Comment,
    int StarRating);

public record SentimentBatchResponse(
    string MatchId,
    string TargetPlayerId,
    List<SentimentItemResult> SentimentDetails);

public record SentimentItemResult(
    string FeedbackId,
    string ReviewerId,
    string Label,
    decimal Score,
    decimal Confidence);

public record EloCalculationRequest(
    string ContractVersion,
    EloCalculationMatch Match,
    EloCalculationPlayer Player,
    EloKFactors KFactors,
    List<EloCalculationFeedback> Feedbacks);

public record EloKFactors(
    decimal K1MatchResult,
    decimal K2FeedbackSentiment,
    decimal K3WinStreak,
    decimal K4Contribution,
    decimal K5Trust);

public record EloCalculationMatch(
    string MatchId,
    decimal Result,
    int TeamElo,
    int OpponentElo,
    int DaysSinceLastMatch);

public record EloCalculationPlayer(
    string PlayerId,
    int CurrentElo,
    int TrustScore,
    bool IsLegend,
    EloRecentMatches RecentMatches,
    EloContribution Contribution);

public record EloRecentMatches(
    int Total,
    int Wins,
    int WinStreak);

public record EloContribution(
    bool SubmittedFeedback,
    bool FullFeedbackCoverage);

public record EloCalculationFeedback(
    string FromPlayerId,
    int StarRating,
    string Comment);

public record EloCalculationResponse(
    string ContractVersion,
    string FormulaVersion,
    string ModelVersion,
    string GeneratedAtUtc,
    string PlayerId,
    string MatchId,
    EloResult Elo,
    EloRankResult Rank,
    EloBreakdown Breakdown,
    List<EloSentimentDetail> SentimentDetails);

public record EloRankResult(
    string Previous,
    string New,
    bool Changed);

public record EloResult(
    int Previous,
    decimal Delta,
    int New);

public record EloBreakdown(
    EloBreakdownComponent K1MatchResult,
    EloBreakdownComponent K2FeedbackSentiment,
    EloBreakdownComponent K3WinStreak,
    EloBreakdownComponent K4Contribution,
    EloBreakdownComponent K5Trust);

public record EloBreakdownComponent(
    decimal Delta,
    Dictionary<string, decimal> Metrics);

public record EloSentimentDetail(
    string FromPlayerId,
    string Label,
    decimal Confidence,
    decimal Score);

public record RadarAnalysisRequest(
    string ContractVersion,
    string PlayerId,
    RadarPlayerProfile PlayerProfile,
    List<RadarRecentMatch> RecentMatches,
    List<RadarFeedbackReceived> FeedbackReceived);

public record RadarPlayerProfile(
    int CurrentElo,
    int TrustScore,
    int TotalMatches,
    int Wins,
    int Losses,
    int Draws,
    int WinStreak,
    int AfkCount,
    int ReportCount,
    string PreferredPositions);

public record RadarRecentMatch(
    string MatchId,
    decimal Result,
    decimal ExpectedScore,
    int DaysAgo,
    bool SubmittedFeedback);

public record RadarFeedbackReceived(
    int StarRating,
    string Comment,
    int DaysAgo);

public record RadarAnalysisResponse(
    string ContractVersion,
    string FormulaVersion,
    string ModelVersion,
    string GeneratedAtUtc,
    string PlayerId,
    RadarAxis Radar,
    List<RadarAssessment> Assessments,
    string Summary);

public record RadarAxis(
    decimal Form,
    decimal WinRate,
    decimal CommunityScore,
    decimal Trust,
    decimal Contribution);

public record RadarAssessment(
    string Type,
    string Title,
    string Description,
    string Icon,
    string HighlightAxis);

public record FeedbackSuggestionRequest(
    int StarRating,
    int Count,
    string? Role);

public record FeedbackSuggestionResponse(
    int StarRating,
    List<string> Suggestions,
    string Source,
    string? ModelUsed,
    string? FallbackReason);
