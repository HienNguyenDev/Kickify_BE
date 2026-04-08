using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.MatchPresets.Commands.CreateMatchPreset
{
    public record CreateMatchPresetCommand(
        Guid FieldId,
        string RoomName,
        string MatchFormat,
        string? Visibility,
        TimeSpan StartTime,
        int DurationMinutes,
        string? Rules,
        string? Password,
        string? Description
    ) : ICommand<CreateMatchPresetResponse>;
}
