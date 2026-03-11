using Kickify.Domain.Common;

namespace Kickify.Domain.Errors;

public static class VenueFeedbackErrors
{
    public static readonly Error CannotFeedbackOwnVenue = Error.Conflict(
        "VenueFeedback.CannotFeedbackOwnVenue",
        "You cannot send feedback to your own venue");
}
