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
        private readonly IFieldRepository _fieldRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserContext _userContext;

        public UpdateMatchPresetCommandHandler(
            IMatchPresetRepository matchPresetRepository,
            IFieldRepository fieldRepository,
            IUnitOfWork unitOfWork,
            IUserContext userContext)
        {
            _matchPresetRepository = matchPresetRepository;
            _fieldRepository = fieldRepository;
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
            if (request.RoomName != null)
            {
                preset.RoomName = request.RoomName;
            }

            if (request.FieldId.HasValue)
            {
                var field = await _fieldRepository.GetByIdAsync(request.FieldId.Value);
                if (field == null)
                {
                    return Result.Failure<UpdateMatchPresetResponse>(FieldErrors.NotFound(request.FieldId.Value));
                }

                preset.FieldId = request.FieldId.Value;
            }

            if (request.Description != null)
            {
                preset.Description = request.Description;
            }

            if (request.Rules != null)
            {
                preset.Rules = request.Rules;
            }

            if (request.StartTime.HasValue)
            {
                preset.StartTime = request.StartTime.Value;
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
                    preset.Password = null;
                }
            }

            if (request.Password != null)
            {
                preset.Password = string.IsNullOrWhiteSpace(request.Password)
                    ? null
                    : request.Password;
            }

            if (preset.Visibility == Visibility.Public)
            {
                preset.Password = null;
            }

            _matchPresetRepository.Update(preset);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(new UpdateMatchPresetResponse(
                preset.PresetId,
                preset.UserId,
                preset.FieldId,
                preset.RoomName,
                preset.MatchFormat.ToString(),
                preset.Visibility.ToString(),
                preset.Password,
                preset.StartTime,
                preset.Rules,
                preset.DurationMinutes,
                preset.Description,
                preset.CreatedAt
            ));
        }
    }
}
