using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.MatchPresets.Commands.CreateMatchPreset
{
    public class CreateMatchPresetCommandHandler : ICommandHandler<CreateMatchPresetCommand, CreateMatchPresetResponse>
    {
        private readonly IMatchPresetRepository _matchPresetRepository;
        private readonly IFieldRepository _fieldRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserContext _userContext;

        public CreateMatchPresetCommandHandler(
            IMatchPresetRepository matchPresetRepository,
            IFieldRepository fieldRepository,
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            IUserContext userContext)
        {
            _matchPresetRepository = matchPresetRepository;
            _fieldRepository = fieldRepository;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _userContext = userContext;
        }

        public async Task<Result<CreateMatchPresetResponse>> Handle(CreateMatchPresetCommand request, CancellationToken cancellationToken)
        {
            var userId = _userContext.UserId;
            
            // Verify user exists
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return Result.Failure<CreateMatchPresetResponse>(UserErrors.NotFound(userId));
            }

            // Parse MatchFormat enum
            if (!Enum.TryParse<MatchFormat>(request.MatchFormat, true, out var matchFormat))
            {
                return Result.Failure<CreateMatchPresetResponse>(MatchPresetErrors.InvalidFormat(request.MatchFormat));
            }

            // Verify field exists if provided
            string? fieldName = null;
            if (request.FieldId.HasValue)
            {
                var field = await _fieldRepository.GetByIdAsync(request.FieldId.Value);
                if (field == null)
                {
                    return Result.Failure<CreateMatchPresetResponse>(FieldErrors.NotFound(request.FieldId.Value));
                }
                fieldName = field.FieldName;
            }

            // Create preset
            var preset = new MatchPreset
            {
                PresetId = Guid.NewGuid(),
                UserId = userId,
                PresetName = request.PresetName,
                FieldId = request.FieldId,
                CustomLocation = request.CustomLocation,
                MatchFormat = matchFormat,
                DurationMinutes = request.DurationMinutes,
                Description = request.Description,
                CreatedAt = DateTime.UtcNow
            };

            await _matchPresetRepository.AddAsync(preset);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(new CreateMatchPresetResponse(
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
