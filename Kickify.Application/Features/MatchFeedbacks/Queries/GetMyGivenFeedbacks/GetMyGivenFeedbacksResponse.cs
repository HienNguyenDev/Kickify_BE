namespace Kickify.Application.Features.MatchFeedbacks.Queries.GetMyGivenFeedbacks;

public record GetMyGivenFeedbacksResponse(
    IReadOnlyList<GivenFeedbackItem> Items,
    int TotalCount,
    int Page,
    int PageSize
);

public record GivenFeedbackItem(
    Guid FeedbackId,
    Guid MatchId,
    Guid RevieweeId,
    string? RevieweeName,
    string? RevieweeAvatarUrl,
    int Rating,
    string Comment,
    decimal? SentimentScore,
    string? SentimentLabel,
    string? RevieweeResponse,
    DateTime? ResponseDate,
    DateTime CreatedAt
);
