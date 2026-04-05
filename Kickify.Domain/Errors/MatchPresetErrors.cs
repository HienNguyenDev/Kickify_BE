using Kickify.Domain.Common;

namespace Kickify.Domain.Errors
{
    public static class MatchPresetErrors
    {
        public static Error NotFound(Guid presetId) => Error.NotFound(
            "MatchPreset.NotFound",
            $"Match preset with ID {presetId} not found");

        public static readonly Error Unauthorized = Error.Failure(
            "MatchPreset.Unauthorized",
            "You are not authorized to access this preset");

        public static Error InvalidFormat(string matchFormat) => Error.Conflict(
            "MatchPreset.InvalidFormat",
            $"Invalid match format: {matchFormat}");

        public static Error InvalidVisibility(string? visibility) => Error.Conflict(
            "MatchPreset.InvalidVisibility",
            $"Invalid visibility: {visibility}");

        public static readonly Error InvalidDuration = Error.Conflict(
            "MatchPreset.InvalidDuration",
            "Duration must be greater than 0 minutes");

        public static readonly Error RoomNameRequired = Error.Conflict(
            "MatchPreset.RoomNameRequired",
            "Room name is required");
    }
}
