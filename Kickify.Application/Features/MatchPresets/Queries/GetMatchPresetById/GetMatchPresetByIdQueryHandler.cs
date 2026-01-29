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

            // Map Field and Venue
            PresetFieldDto? fieldDto = null;
            if (preset.Field != null)
            {
                var venueDto = new PresetVenueDto(
                    preset.Field.Venue.VenueId,
                    preset.Field.Venue.VenueName,
                    preset.Field.Venue.Address
                );

                fieldDto = new PresetFieldDto(
                    preset.Field.FieldId,
                    preset.Field.FieldName,
                    preset.Field.FieldType.ToString(),
                    venueDto
                );
            }

            return Result.Success(new GetMatchPresetByIdResponse(
                preset.PresetId,
                preset.UserId,
                preset.User.FullName ?? "Unknown",
                preset.PresetName,
                preset.FieldId,
                fieldDto,
                preset.CustomLocation,
                preset.MatchFormat.ToString(),
                preset.DurationMinutes,
                preset.Description,
                preset.CreatedAt
            ));
        }
    }
}
