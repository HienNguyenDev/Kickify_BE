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

        public static Error InvalidFormat(string matchFormat) => Error.Problem(
            "MatchPreset.InvalidFormat",
            $"Invalid match format: {matchFormat}");

        public static readonly Error InvalidDuration = Error.Problem(
            "MatchPreset.InvalidDuration",
            "Duration must be greater than 0 minutes");

        public static readonly Error PresetNameRequired = Error.Problem(
            "MatchPreset.PresetNameRequired",
            "Preset name is required");
    }
}
