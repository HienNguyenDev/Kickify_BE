using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Abstractions.Services;
using Kickify.Domain.Common;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace Kickify.Application.Features.MatchRooms.Commands.UpdateRoomPrivacy
{
    public class UpdateRoomPrivacyCommandHandler : ICommandHandler<UpdateRoomPrivacyCommand, UpdateRoomPrivacyResponse>
    {
        private readonly IMatchRoomRepository _matchRoomRepository;
        private readonly IMatchRoomHubService _matchRoomHubService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserContext _userContext;
        private readonly ILogger<UpdateRoomPrivacyCommandHandler> _logger;

        public UpdateRoomPrivacyCommandHandler(
            IMatchRoomRepository matchRoomRepository,
            IMatchRoomHubService matchRoomHubService,
            IUnitOfWork unitOfWork,
            IUserContext userContext,
            ILogger<UpdateRoomPrivacyCommandHandler> logger)
        {
            _matchRoomRepository = matchRoomRepository;
            _matchRoomHubService = matchRoomHubService;
            _unitOfWork = unitOfWork;
            _userContext = userContext;
            _logger = logger;
        }

        public async Task<Result<UpdateRoomPrivacyResponse>> Handle(UpdateRoomPrivacyCommand request, CancellationToken cancellationToken)
        {
            var userId = _userContext.UserId;

            // Get room
            var room = await _matchRoomRepository.GetByIdAsync(request.RoomId);
            if (room == null)
            {
                return Result.Failure<UpdateRoomPrivacyResponse>(MatchRoomErrors.NotFound(request.RoomId));
            }

            // Only host can update privacy settings
            if (room.HostId != userId)
            {
                return Result.Failure<UpdateRoomPrivacyResponse>(MatchRoomErrors.OnlyHostCanUpdatePrivacy);
            }

            // Check if room is still active (Open status)
            if (room.Status != RoomStatus.Open)
            {
                return Result.Failure<UpdateRoomPrivacyResponse>(MatchRoomErrors.RoomNotActive);
            }

            // Parse Visibility enum
            if (!Enum.TryParse<Visibility>(request.Visibility, true, out var visibility))
            {
                return Result.Failure<UpdateRoomPrivacyResponse>(
                    new Error("MatchRoom.InvalidVisibility", "Visibility must be Public or Private", ErrorType.Validation));
            }

            var previousVisibility = room.Visibility;

            // Update room privacy
            room.Visibility = visibility;
            room.RoomPassword = visibility == Visibility.Private ? request.Password : null;

            _matchRoomRepository.Update(room);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var updatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

            _logger.LogInformation("Room {RoomId} privacy updated from {PreviousVisibility} to {NewVisibility} by host {UserId}",
                room.RoomId, previousVisibility, visibility, userId);

            // Send real-time notification to all room participants
            await _matchRoomHubService.NotifyRoomPrivacyUpdatedAsync(
                room.RoomId,
                visibility.ToString(),
                visibility == Visibility.Private,
                cancellationToken);

            return Result.Success(new UpdateRoomPrivacyResponse(
                room.RoomId,
                room.Visibility.ToString(),
                room.Visibility == Visibility.Private,
                updatedAt
            ));
        }
    }
}
