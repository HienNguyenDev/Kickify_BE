using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.MatchPresets.Commands.CreateMatchPreset
{
    public record CreateMatchPresetCommand(
        string PresetRoomName,
        string MatchFormat,
        string? Visibility,
        int DurationMinutes,
        string? RoomPassword,
        string? Description
    ) : ICommand<CreateMatchPresetResponse>;
}
