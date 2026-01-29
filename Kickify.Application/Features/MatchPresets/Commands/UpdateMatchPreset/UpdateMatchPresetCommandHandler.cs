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

        public UpdateMatchPresetCommandHandler(
            IMatchPresetRepository matchPresetRepository,
            IFieldRepository fieldRepository,
            IUnitOfWork unitOfWork)
        {
            _matchPresetRepository = matchPresetRepository;
            _fieldRepository = fieldRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<UpdateMatchPresetResponse>> Handle(UpdateMatchPresetCommand request, CancellationToken cancellationToken)
        {
            // Get preset
            var preset = await _matchPresetRepository.GetByIdAsync(request.PresetId, cancellationToken);
            if (preset == null)
            {
                return Result.Failure<UpdateMatchPresetResponse>(MatchPresetErrors.NotFound(request.PresetId));
            }

            // Check ownership
            if (preset.UserId != request.UserId)
            {
                return Result.Failure<UpdateMatchPresetResponse>(MatchPresetErrors.Unauthorized);
            }

            // Update fields if provided (null = keep old value)
            if (request.PresetName != null)
            {
                preset.PresetName = request.PresetName;
            }

            if (request.CustomLocation != null)
            {
                preset.CustomLocation = request.CustomLocation;
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

            // Handle FieldId update
            string? fieldName = null;
            if (request.FieldId.HasValue)
            {
                var field = await _fieldRepository.GetByIdAsync(request.FieldId.Value);
                if (field == null)
                {
                    return Result.Failure<UpdateMatchPresetResponse>(FieldErrors.NotFound(request.FieldId.Value));
                }
                preset.FieldId = request.FieldId.Value;
                fieldName = field.FieldName;
            }
            else if (preset.FieldId.HasValue)
            {
                // Get existing field name
                var field = await _fieldRepository.GetByIdAsync(preset.FieldId.Value);
                fieldName = field?.FieldName;
            }

            _matchPresetRepository.Update(preset);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(new UpdateMatchPresetResponse(
                preset.PresetId,
                preset.UserId,
                preset.PresetName,
                preset.FieldId,
                fieldName,
                preset.CustomLocation,
                preset.MatchFormat.ToString(),
                preset.DurationMinutes,
                preset.Description,
                preset.CreatedAt
            ));
        }
    }
}
