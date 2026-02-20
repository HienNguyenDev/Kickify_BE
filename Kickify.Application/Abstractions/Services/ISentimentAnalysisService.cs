namespace Kickify.Application.Abstractions.Services;

public interface ISentimentAnalysisService
{
    /// <summary>
    /// Send a batch of feedbacks for a specific player in a match to the AI sentiment analysis API.
    /// </summary>
    Task SendFeedbacksForAnalysisAsync(
        SentimentBatchRequest request,
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
