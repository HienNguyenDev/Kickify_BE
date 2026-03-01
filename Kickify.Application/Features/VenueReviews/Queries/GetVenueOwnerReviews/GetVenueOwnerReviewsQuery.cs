using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.VenueReviews.Queries.GetVenueOwnerReviews;

public record GetVenueOwnerReviewsQuery(
    Guid? VenueId = null,
    int? MinRating = null,
    int? MaxRating = null,
    int Page = 1,
    int PageSize = 10
) : IQuery<GetVenueOwnerReviewsResponse>;
