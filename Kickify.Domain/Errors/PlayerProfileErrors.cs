using Kickify.Domain.Common;

namespace Kickify.Domain.Errors
{
    public static class PlayerProfileErrors
    {
        public static Error NotFound(Guid? profileId) => Error.NotFound(
            "PlayerProfiles.NotFound",
            $"The player profile with the Id = '{profileId}' was not found");

        public static Error NotFoundByUserId(Guid userId) => Error.NotFound(
            "PlayerProfiles.NotFoundByUserId",
            $"The player profile for user with Id = '{userId}' was not found");

        public static Error AlreadyExists(Guid userId) => Error.Conflict(
            "PlayerProfiles.AlreadyExists",
            $"The player profile for user with Id = '{userId}' already exists");

        public static readonly Error InvalidElo = Error.Problem(
            "PlayerProfiles.InvalidElo",
            "The ELO rating must be between 0 and 5000");

        public static readonly Error InvalidTrustScore = Error.Problem(
            "PlayerProfiles.InvalidTrustScore",
            "The trust score must be between 0 and 100");

        public static readonly Error InvalidStats = Error.Problem(
            "PlayerProfiles.InvalidStats",
            "Player statistics cannot be negative");

        public static readonly Error UserNotFound = Error.NotFound(
            "PlayerProfiles.UserNotFound",
            "The user associated with this profile was not found");
    }
}
