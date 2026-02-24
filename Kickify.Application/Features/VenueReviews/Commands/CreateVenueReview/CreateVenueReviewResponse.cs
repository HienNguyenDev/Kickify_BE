namespace Kickify.Application.Features.VenueReviews.Commands.CreateVenueReview;

public record CreateVenueReviewResponse(
    Guid ReviewId,
    Guid VenueId,
    string VenueName,
    Guid BookingId,
    int Rating,
    string? Comment,
    DateTime CreatedAt
);
