using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.MatchPresets.Queries.GetMatchPresetById
{
    public record GetMatchPresetByIdQuery(Guid PresetId) : IQuery<GetMatchPresetByIdResponse>;
}
