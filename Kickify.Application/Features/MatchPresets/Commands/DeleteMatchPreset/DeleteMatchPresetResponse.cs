namespace Kickify.Application.Features.MatchPresets.Commands.DeleteMatchPreset
{
    public record DeleteMatchPresetResponse(
        Guid PresetId,
        string Message
    );
}
