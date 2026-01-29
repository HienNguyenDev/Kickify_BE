using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.MatchPresets.Commands.CreateMatchPreset
{
    public record CreateMatchPresetCommand(
        Guid UserId,
        string PresetName,
        Guid? FieldId,
        string? CustomLocation,
        string MatchFormat,
        int DurationMinutes,
        string? Description
    ) : ICommand<CreateMatchPresetResponse>;
}
