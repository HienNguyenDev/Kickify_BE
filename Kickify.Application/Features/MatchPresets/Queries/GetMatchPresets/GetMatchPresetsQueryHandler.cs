using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;

namespace Kickify.Application.Features.MatchPresets.Queries.GetMatchPresets
{
    public class GetMatchPresetsQueryHandler : IQueryHandler<GetMatchPresetsQuery, GetMatchPresetsResponse>
    {
        private readonly IMatchPresetRepository _matchPresetRepository;

        public GetMatchPresetsQueryHandler(IMatchPresetRepository matchPresetRepository)
        {
            _matchPresetRepository = matchPresetRepository;
        }

        public async Task<Result<GetMatchPresetsResponse>> Handle(GetMatchPresetsQuery request, CancellationToken cancellationToken)
        {
            var (presets, total) = await _matchPresetRepository.GetAllPagedAsync(
                request.Page,
                request.PageSize,
                cancellationToken);

            var items = presets.Select(p => new MatchPresetItemDto(
                p.PresetId,
                p.UserId,
                p.User?.FullName ?? "Unknown",
                p.PresetName,
                p.FieldId,
                p.Field?.FieldName,
                p.Field?.Venue?.VenueName,
                p.CustomLocation,
                p.MatchFormat.ToString(),
                p.DurationMinutes,
                p.Description,
                p.CreatedAt
            )).ToList();

            var totalPages = (int)Math.Ceiling((double)total / request.PageSize);

            return Result.Success(new GetMatchPresetsResponse(
                items,
                request.Page,
                request.PageSize,
                total,
                totalPages
            ));
        }
    }
}
