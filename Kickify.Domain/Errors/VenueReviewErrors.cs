using Kickify.Domain.Common;

namespace Kickify.Domain.Errors
{
    public static class VenueReviewErrors
    {
        public static Error BookingNotFound(Guid bookingId) => Error.NotFound(
            "VenueReviews.BookingNotFound",
            $"The booking with Id = '{bookingId}' was not found");

        public static readonly Error NotParticipant = Error.Problem(
            "VenueReviews.NotParticipant",
            "You are not a participant in this match room and cannot review this venue");

        public static readonly Error MatchNotEnded = Error.Problem(
            "VenueReviews.MatchNotEnded",
            "You can only review a venue after the match has ended");

        public static readonly Error BookingNotCompleted = Error.Problem(
            "VenueReviews.BookingNotCompleted",
            "The booking or match room has not been completed yet");

        public static readonly Error AlreadyReviewed = Error.Conflict(
            "VenueReviews.AlreadyReviewed",
            "You have already reviewed this venue for this booking");

        public static readonly Error InvalidRating = Error.Problem(
            "VenueReviews.InvalidRating",
            "Rating must be between 1 and 5");
    }
}
