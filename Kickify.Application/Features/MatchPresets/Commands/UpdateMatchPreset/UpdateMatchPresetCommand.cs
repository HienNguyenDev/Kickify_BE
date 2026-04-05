using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.MatchPresets.Commands.UpdateMatchPreset
{
    public record UpdateMatchPresetCommand(
        Guid PresetId,
        Guid? FieldId,
        string? RoomName,
        string? MatchFormat,
        string? Visibility,
        TimeSpan? StartTime,
        int? DurationMinutes,
        string? Rules,
        string? Password,
        string? Description
    ) : ICommand<UpdateMatchPresetResponse>;
}
