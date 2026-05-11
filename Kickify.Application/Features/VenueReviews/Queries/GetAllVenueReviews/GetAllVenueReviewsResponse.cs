namespace Kickify.Application.Features.VenueReviews.Queries.GetAllVenueReviews;

public record GetAllVenueReviewsResponse(
    List<VenueReviewItemDto> Reviews,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);

public record VenueReviewItemDto(
    Guid ReviewId,
    Guid VenueId,
    string VenueName,
    Guid UserId,
    string? UserFullName,
    string? UserAvatarUrl,
    Guid BookingId,
    int Rating,
    string? Comment,
    string? OwnerResponse,
    DateTime? ResponseDate,
    DateTime CreatedAt
);
