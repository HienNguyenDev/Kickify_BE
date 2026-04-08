namespace Kickify.Application.Features.MatchFeedbacks.Queries.GetMyReceivedFeedbacks;

public record GetMyReceivedFeedbacksResponse(
    IReadOnlyList<ReceivedFeedbackItem> Items,
    int TotalCount,
    int Page,
    int PageSize
);

public record ReceivedFeedbackItem(
    Guid FeedbackId,
    Guid MatchId,
    Guid ReviewerId,
    string? ReviewerName,
    string? ReviewerAvatarUrl,
    int Rating,
    string Comment,
    decimal? SentimentScore,
    string? SentimentLabel,
    string? RevieweeResponse,
    DateTime? ResponseDate,
    DateTime CreatedAt
);
