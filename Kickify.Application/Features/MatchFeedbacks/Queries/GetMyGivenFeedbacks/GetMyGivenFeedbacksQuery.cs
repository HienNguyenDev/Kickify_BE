using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.MatchFeedbacks.Queries.GetMyGivenFeedbacks;

public record GetMyGivenFeedbacksQuery(
    DateTime? FromDate,
    DateTime? ToDate,
    int? Rating,
    int Page,
    int PageSize) : IQuery<GetMyGivenFeedbacksResponse>;
