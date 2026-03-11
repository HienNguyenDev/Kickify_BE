using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.VenueFeedbacks.Queries.GetVenueFeedbacks;

public record GetVenueFeedbacksQuery(
    Guid VenueId,
    int Page = 1,
    int PageSize = 10
) : IQuery<GetVenueFeedbacksResponse>;
