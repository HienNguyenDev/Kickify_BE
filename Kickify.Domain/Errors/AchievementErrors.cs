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

    public static readonly Error IconUploadFailed = Error.Problem(
        "Achievement.IconUploadFailed",
        "Failed to upload badge icon");

    public static readonly Error InvalidCriteriaType = Error.Problem(
        "Achievement.InvalidCriteriaType",
        "Invalid criteria type");
}
