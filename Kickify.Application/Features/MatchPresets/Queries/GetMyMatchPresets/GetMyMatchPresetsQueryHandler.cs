using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;

namespace Kickify.Application.Features.MatchPresets.Queries.GetMyMatchPresets
{
    public class GetMyMatchPresetsQueryHandler : IQueryHandler<GetMyMatchPresetsQuery, GetMyMatchPresetsResponse>
    {
        private readonly IMatchPresetRepository _matchPresetRepository;
        private readonly IUserContext _userContext;

        public GetMyMatchPresetsQueryHandler(
            IMatchPresetRepository matchPresetRepository,
            IUserContext userContext)
        {
            _matchPresetRepository = matchPresetRepository;
            _userContext = userContext;
        }

        public async Task<Result<GetMyMatchPresetsResponse>> Handle(GetMyMatchPresetsQuery request, CancellationToken cancellationToken)
        {
            var userId = _userContext.UserId;
            
            var (presets, total) = await _matchPresetRepository.GetByUserIdPagedAsync(
                userId,
                request.Page,
                request.PageSize,
                cancellationToken);

            var items = presets.Select(p => new MyMatchPresetItemDto(
                p.PresetId,
                p.FieldId,
                p.Field?.FieldName,
                p.Field?.Venue?.VenueName,
                p.Field?.Venue?.Address,
                p.RoomName,
                p.MatchFormat.ToString(),
                p.Visibility.ToString(),
                p.Password,
                p.StartTime,
                p.Rules,
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
