using Kickify.Application.Features.VenueReviews.Queries.GetAllVenueReviews;

namespace Kickify.Application.Features.VenueReviews.Queries.GetVenueOwnerReviews;

public record GetVenueOwnerReviewsResponse(
    List<VenueReviewItemDto> Reviews,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);
