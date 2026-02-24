using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.MatchPresets.Commands.DeleteMatchPreset
{
    public record DeleteMatchPresetCommand(
        Guid PresetId
    ) : ICommand<DeleteMatchPresetResponse>;
}
