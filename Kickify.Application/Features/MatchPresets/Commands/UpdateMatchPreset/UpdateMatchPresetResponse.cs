namespace Kickify.Application.Features.MatchPresets.Commands.UpdateMatchPreset
{
    public record UpdateMatchPresetResponse(
        Guid PresetId,
        Guid UserId,
        string PresetName,
        Guid? FieldId,
        string? FieldName,
        string? CustomLocation,
        string MatchFormat,
        int DurationMinutes,
        string? Description,
        DateTime CreatedAt
    );
}
