using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;

namespace Kickify.Application.Features.MatchPresets.Queries.GetMyMatchPresets
{
    public class GetMyMatchPresetsQueryHandler : IQueryHandler<GetMyMatchPresetsQuery, GetMyMatchPresetsResponse>
    {
        private readonly IMatchPresetRepository _matchPresetRepository;

        public GetMyMatchPresetsQueryHandler(IMatchPresetRepository matchPresetRepository)
        {
            _matchPresetRepository = matchPresetRepository;
        }

        public async Task<Result<GetMyMatchPresetsResponse>> Handle(GetMyMatchPresetsQuery request, CancellationToken cancellationToken)
        {
            var (presets, total) = await _matchPresetRepository.GetByUserIdPagedAsync(
                request.UserId,
                request.Page,
                request.PageSize,
                cancellationToken);

            var items = presets.Select(p => new MyMatchPresetItemDto(
                p.PresetId,
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

            return Result.Success(new GetMyMatchPresetsResponse(
                items,
                request.Page,
                request.PageSize,
                total,
                totalPages
            ));
        }
    }
}
