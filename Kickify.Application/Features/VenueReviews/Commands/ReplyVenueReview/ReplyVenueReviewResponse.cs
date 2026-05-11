namespace Kickify.Application.Features.VenueReviews.Commands.ReplyVenueReview;

public record ReplyVenueReviewResponse(
    Guid ReviewId,
    Guid VenueId,
    string OwnerResponse,
    DateTime ResponseDateUtc);
