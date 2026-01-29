using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.MatchPresets.Commands.UpdateMatchPreset
{
    public record UpdateMatchPresetCommand(
        Guid UserId,
        Guid PresetId,
        string? PresetName,
        Guid? FieldId,
        string? CustomLocation,
        string? MatchFormat,
        int? DurationMinutes,
        string? Description
    ) : ICommand<UpdateMatchPresetResponse>;
}
