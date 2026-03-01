using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.VenueReviews.Commands.DeleteVenueReview;

public record DeleteVenueReviewCommand(Guid ReviewId) : ICommand<DeleteVenueReviewResponse>;
