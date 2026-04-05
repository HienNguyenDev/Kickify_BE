namespace Kickify.Application.Features.MatchPresets.Commands.CreateMatchPreset
{
    public record CreateMatchPresetResponse(
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
