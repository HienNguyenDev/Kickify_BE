using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.VenueReviews.Commands.CreateVenueReview;

public record CreateVenueReviewCommand(
    Guid VenueId,
    int Rating,
    string? Comment
) : ICommand<CreateVenueReviewResponse>;
