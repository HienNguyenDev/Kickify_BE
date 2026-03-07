using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.VenueReviews.Queries.GetAllVenueReviews;

public record GetAllVenueReviewsQuery(
    Guid? VenueId = null,
    Guid? UserId = null,
    int? MinRating = null,
    int? MaxRating = null,
    int Page = 1,
    int PageSize = 10
) : IQuery<GetAllVenueReviewsResponse>;
