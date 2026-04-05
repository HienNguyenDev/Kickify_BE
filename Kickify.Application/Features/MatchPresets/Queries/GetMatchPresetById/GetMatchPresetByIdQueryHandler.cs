using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.MatchPresets.Queries.GetMatchPresetById
{
    public class GetMatchPresetByIdQueryHandler : IQueryHandler<GetMatchPresetByIdQuery, GetMatchPresetByIdResponse>
    {
        private readonly IMatchPresetRepository _matchPresetRepository;

        public GetMatchPresetByIdQueryHandler(IMatchPresetRepository matchPresetRepository)
        {
            _matchPresetRepository = matchPresetRepository;
        }

        public async Task<Result<GetMatchPresetByIdResponse>> Handle(GetMatchPresetByIdQuery request, CancellationToken cancellationToken)
        {
            var preset = await _matchPresetRepository.GetByIdWithDetailsAsync(request.PresetId, cancellationToken);

            if (preset == null)
            {
                return Result.Failure<GetMatchPresetByIdResponse>(MatchPresetErrors.NotFound(request.PresetId));
            }

            return Result.Success(new GetMatchPresetByIdResponse(
                preset.PresetId,
                preset.UserId,
                preset.User.FullName ?? "Unknown",
                preset.PresetRoomName,
                preset.MatchFormat.ToString(),
                preset.Visibility.ToString(),
                preset.RoomPassword,
                preset.DurationMinutes,
                preset.Description,
                preset.CreatedAt
            ));
        }
    }
}
