using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Features.VenueReviews.Commands.ReplyVenueReview;

namespace Kickify.Application.Features.VenueReviews.Commands.UpdateReplyVenueReview;

public record UpdateReplyVenueReviewCommand(Guid ReviewId, string Message)
    : ICommand<ReplyVenueReviewResponse>;
