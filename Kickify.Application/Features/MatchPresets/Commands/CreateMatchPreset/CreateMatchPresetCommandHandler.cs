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
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserContext _userContext;

        public CreateMatchPresetCommandHandler(
            IMatchPresetRepository matchPresetRepository,
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            IUserContext userContext)
        {
            _matchPresetRepository = matchPresetRepository;
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

            var visibilityInput = string.IsNullOrWhiteSpace(request.Visibility)
                ? Visibility.Public.ToString()
                : request.Visibility;

            if (!Enum.TryParse<Visibility>(visibilityInput, true, out var visibility))
            {
                return Result.Failure<CreateMatchPresetResponse>(MatchPresetErrors.InvalidVisibility(request.Visibility));
            }

            // Create preset
            var preset = new MatchPreset
            {
                PresetId = Guid.NewGuid(),
                UserId = userId,
                PresetRoomName = request.PresetRoomName,
                MatchFormat = matchFormat,
                Visibility = visibility,
                RoomPassword = visibility == Visibility.Private
                    ? (string.IsNullOrWhiteSpace(request.RoomPassword) ? null : request.RoomPassword)
                    : null,
                DurationMinutes = request.DurationMinutes,
                Description = request.Description,
                CreatedAt = DateTime.UtcNow
            };

            await _matchPresetRepository.AddAsync(preset);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(new CreateMatchPresetResponse(
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
