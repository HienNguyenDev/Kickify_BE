namespace Kickify.Application.Features.MatchPresets.Commands.UpdateMatchPreset
{
    public record UpdateMatchPresetResponse(
        Guid PresetId,
        Guid UserId,
        string PresetRoomName,
        string MatchFormat,
        string Visibility,
        string? RoomPassword,
        int DurationMinutes,
        string? Description,
        DateTime CreatedAt
    );
}
