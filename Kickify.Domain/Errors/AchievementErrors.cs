using Kickify.Domain.Common;

namespace Kickify.Domain.Errors;

public static class AchievementErrors
{
    public static Error NotFound(Guid achievementId) => Error.NotFound(
        "Achievement.NotFound",
        $"Achievement with ID {achievementId} not found");

    public static readonly Error NameAlreadyExists = Error.Conflict(
        "Achievement.NameAlreadyExists",
        "An achievement with this name already exists");

    public static readonly Error IconUploadFailed = Error.Conflict(
        "Achievement.IconUploadFailed",
        "Failed to upload badge icon");

    public static readonly Error InvalidCriteriaType = Error.Conflict(
        "Achievement.InvalidCriteriaType",
        "Invalid criteria type");

    public static readonly Error AlreadyClaimed = Error.Conflict(
        "Achievement.AlreadyClaimed",
        "Achievement has already been claimed");

    public static Error ClaimConditionNotMet(string achievementName) => Error.Conflict(
        "Achievement.ClaimConditionNotMet",
        $"Claim conditions are not met for achievement '{achievementName}'");
}
