namespace Kickify.Application.Features.MatchPresets.Commands.CreateMatchPreset
{
    public record CreateMatchPresetResponse(
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
