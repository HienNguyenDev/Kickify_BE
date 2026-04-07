using Kickify.Domain.Common;

namespace Kickify.Domain.Errors;

public static class MatchFeedbackErrors
{
    public static readonly Error Forbidden = Error.Failure("MatchFeedback.Forbidden", "You do not have permission to access this feedback");
    public static readonly Error MatchNotReviewing = Error.Conflict("MatchFeedback.MatchNotReviewing", "Match must be end before giving feedback");
    public static readonly Error AlreadyReviewed = Error.Conflict("MatchFeedback.AlreadyReviewed", "You have already reviewed this player for this match");
    public static readonly Error CannotReviewYourself = Error.Conflict("MatchFeedback.CannotReviewYourself", "You cannot review yourself");
    public static readonly Error RevieweeNotInMatch = Error.Conflict("MatchFeedback.RevieweeNotInMatch", "The reviewee was not a participant in this match");
    public static readonly Error ReviewerNotInMatch = Error.Conflict("MatchFeedback.ReviewerNotInMatch", "You were not a participant in this match");
    public static readonly Error InvalidRating = Error.Conflict("MatchFeedback.InvalidRating", "Rating must be between 1 and 5");
    public static Error NotFound(Guid feedbackId) => Error.NotFound("MatchFeedback.NotFound", $"Feedback with Id '{feedbackId}' was not found");
}
