using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.MatchPresets.Commands.UpdateMatchPreset
{
    public record UpdateMatchPresetCommand(
        Guid PresetId,
        string? PresetRoomName,
        string? MatchFormat,
        string? Visibility,
        int? DurationMinutes,
        string? RoomPassword,
        string? Description
    ) : ICommand<UpdateMatchPresetResponse>;
}
