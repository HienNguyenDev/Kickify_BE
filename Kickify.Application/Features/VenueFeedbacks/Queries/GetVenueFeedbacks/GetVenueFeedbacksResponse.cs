namespace Kickify.Application.Features.VenueFeedbacks.Queries.GetVenueFeedbacks;

public record FeedbackItemDto(
    Guid VenueFeedbackId,
    Guid SenderId,
    string SenderName,
    string SenderAvatar,
    string Message,
    int Rating,
    DateTime CreatedAt);

public record GetVenueFeedbacksResponse(
    List<FeedbackItemDto> Feedbacks,
    int Total,
    int Page,
    int PageSize,
    int TotalPages);
