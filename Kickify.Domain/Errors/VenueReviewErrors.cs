using Kickify.Domain.Common;

namespace Kickify.Domain.Errors
{
    public static class VenueReviewErrors
    {
        public static Error NotFound(Guid reviewId) => Error.NotFound(
            "VenueReviews.NotFound",
            $"The venue review with Id = '{reviewId}' was not found");

        public static readonly Error NotVenueOwner = Error.Failure(
            "VenueReviews.NotVenueOwner",
            "You must be a venue owner to access this resource");

        public static Error BookingNotFound(Guid bookingId) => Error.NotFound(
            "VenueReviews.BookingNotFound",
            $"The booking with Id = '{bookingId}' was not found");

        public static readonly Error NotParticipant = Error.Conflict(
            "VenueReviews.NotParticipant",
            "You are not a participant in this match room and cannot review this venue");

        public static readonly Error MatchNotEnded = Error.Conflict(
            "VenueReviews.MatchNotEnded",
            "You can only review a venue after the match has ended");

        public static readonly Error BookingNotCompleted = Error.Conflict(
            "VenueReviews.BookingNotCompleted",
            "The booking or match room has not been completed yet");

        public static readonly Error AlreadyReviewed = Error.Conflict(
            "VenueReviews.AlreadyReviewed",
            "You have already reviewed this venue for this booking");

        public static readonly Error InvalidRating = Error.Conflict(
            "VenueReviews.InvalidRating",
            "Rating must be between 1 and 5");
        public static readonly Error NotEligible = Error.Conflict(
            "VenueReviews.NotEligible",
            "You are not eligible to review this venue. You must have a completed match that hasn't been reviewed yet.");

        public static readonly Error NotOwnerOfVenue = Error.Failure(
            "VenueReviews.NotOwnerOfVenue",
            "You can only reply to reviews for venues you own.");

        public static readonly Error AlreadyReplied = Error.Conflict(
            "VenueReviews.AlreadyReplied",
            "A reply has already been posted for this review.");

        public static readonly Error ReplyRequired = Error.Failure(
            "VenueReviews.ReplyRequired",
            "Reply text is required.");

        public static readonly Error NoReplyToUpdate = Error.Conflict(
            "VenueReviews.NoReplyToUpdate",
            "There is no reply to update. Create a reply first.");
    }
}
