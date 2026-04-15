using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.VenueReviews.Commands.ReplyVenueReview;

public record ReplyVenueReviewCommand(Guid ReviewId, string Message) : ICommand<ReplyVenueReviewResponse>;
