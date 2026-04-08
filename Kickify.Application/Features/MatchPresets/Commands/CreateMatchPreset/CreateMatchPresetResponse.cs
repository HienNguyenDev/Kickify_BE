namespace Kickify.Application.Features.MatchPresets.Commands.CreateMatchPreset
{
    public record CreateMatchPresetResponse(
        Guid PresetId,
        Guid UserId,
        Guid? FieldId,
        string RoomName,
        string MatchFormat,
        string Visibility,
        string? Password,
        TimeSpan StartTime,
        string? Rules,
        int DurationMinutes,
        string? Description,
        DateTime CreatedAt
    );
}
