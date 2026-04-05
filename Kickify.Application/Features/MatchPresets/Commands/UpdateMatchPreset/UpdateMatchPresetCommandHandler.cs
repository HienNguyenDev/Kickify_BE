using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.MatchPresets.Commands.UpdateMatchPreset
{
    public class UpdateMatchPresetCommandHandler : ICommandHandler<UpdateMatchPresetCommand, UpdateMatchPresetResponse>
    {
        private readonly IMatchPresetRepository _matchPresetRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserContext _userContext;

        public UpdateMatchPresetCommandHandler(
            IMatchPresetRepository matchPresetRepository,
            IUnitOfWork unitOfWork,
            IUserContext userContext)
        {
            _matchPresetRepository = matchPresetRepository;
            _unitOfWork = unitOfWork;
            _userContext = userContext;
        }

        public async Task<Result<UpdateMatchPresetResponse>> Handle(UpdateMatchPresetCommand request, CancellationToken cancellationToken)
        {
            var userId = _userContext.UserId;
            
            // Get preset
            var preset = await _matchPresetRepository.GetByIdAsync(request.PresetId, cancellationToken);
            if (preset == null)
            {
                return Result.Failure<UpdateMatchPresetResponse>(MatchPresetErrors.NotFound(request.PresetId));
            }

            // Check ownership
            if (preset.UserId != userId)
            {
                return Result.Failure<UpdateMatchPresetResponse>(MatchPresetErrors.Unauthorized);
            }

            // Update fields if provided (null = keep old value)
            if (request.PresetRoomName != null)
            {
                preset.PresetRoomName = request.PresetRoomName;
            }

            if (request.Description != null)
            {
                preset.Description = request.Description;
            }

            if (request.DurationMinutes.HasValue)
            {
                preset.DurationMinutes = request.DurationMinutes.Value;
            }

            if (request.MatchFormat != null)
            {
                if (!Enum.TryParse<MatchFormat>(request.MatchFormat, true, out var matchFormat))
                {
                    return Result.Failure<UpdateMatchPresetResponse>(MatchPresetErrors.InvalidFormat(request.MatchFormat));
                }
                preset.MatchFormat = matchFormat;
            }

            if (request.Visibility != null)
            {
                if (!Enum.TryParse<Visibility>(request.Visibility, true, out var visibility))
                {
                    return Result.Failure<UpdateMatchPresetResponse>(MatchPresetErrors.InvalidVisibility(request.Visibility));
                }

                preset.Visibility = visibility;

                if (visibility == Visibility.Public)
                {
                    preset.RoomPassword = null;
                }
            }

            if (request.RoomPassword != null)
            {
                preset.RoomPassword = string.IsNullOrWhiteSpace(request.RoomPassword)
                    ? null
                    : request.RoomPassword;
            }

            if (preset.Visibility == Visibility.Public)
            {
                preset.RoomPassword = null;
            }

            _matchPresetRepository.Update(preset);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(new UpdateMatchPresetResponse(
                preset.PresetId,
                preset.UserId,
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
