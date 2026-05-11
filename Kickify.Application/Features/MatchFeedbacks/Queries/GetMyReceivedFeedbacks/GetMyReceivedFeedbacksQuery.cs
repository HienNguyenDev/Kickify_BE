using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.MatchFeedbacks.Queries.GetMyReceivedFeedbacks;

public record GetMyReceivedFeedbacksQuery(
    DateTime? FromDate,
    DateTime? ToDate,
    int? Rating,
    int Page,
    int PageSize) : IQuery<GetMyReceivedFeedbacksResponse>;
