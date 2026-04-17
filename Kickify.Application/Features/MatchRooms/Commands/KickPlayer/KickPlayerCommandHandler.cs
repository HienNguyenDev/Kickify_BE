using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Abstractions.Services;
using Kickify.Domain.Common;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;
using Kickify.Domain.Event;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Kickify.Application.Features.MatchRooms.Commands.KickPlayer
{
    public class KickPlayerCommandHandler : ICommandHandler<KickPlayerCommand, KickPlayerResponse>
    {
        private readonly IMatchRoomRepository _matchRoomRepository;
        private readonly IRoomParticipantRepository _roomParticipantRepository;
        private readonly IMatchRoomHubService _matchRoomHubService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserContext _userContext;
        private readonly IPublisher _publisher;
        private readonly ILogger<KickPlayerCommandHandler> _logger;

        public KickPlayerCommandHandler(
            IMatchRoomRepository matchRoomRepository,
            IRoomParticipantRepository roomParticipantRepository,
            IMatchRoomHubService matchRoomHubService,
            IUnitOfWork unitOfWork,
            IUserContext userContext,
            IPublisher publisher,
            ILogger<KickPlayerCommandHandler> logger)
        {
            _matchRoomRepository = matchRoomRepository;
            _roomParticipantRepository = roomParticipantRepository;
            _matchRoomHubService = matchRoomHubService;
            _unitOfWork = unitOfWork;
            _userContext = userContext;
            _publisher = publisher;
            _logger = logger;
        }

        public async Task<Result<KickPlayerResponse>> Handle(KickPlayerCommand request, CancellationToken cancellationToken)
        {
            var hostId = _userContext.UserId;
            
            // 1. Get room with participants (WITH TRACKING for update/delete)
            var room = await _matchRoomRepository.GetRoomWithParticipantsForUpdateAsync(request.RoomId, cancellationToken);
            if (room == null)
            {
                return Result.Failure<KickPlayerResponse>(MatchRoomErrors.NotFound(request.RoomId));
            }

            // 2. Check if current user is Host
            if (room.HostId != hostId)
            {
                _logger.LogWarning("User {UserId} attempted to kick player but is not host of room {RoomId}",
                    hostId, request.RoomId);
                return Result.Failure<KickPlayerResponse>(MatchRoomErrors.OnlyHostCanKick);
            }

            // 3. Check room status - cannot kick from completed/cancelled rooms
            //if (room.Status == RoomStatus.Completed || room.Status == RoomStatus.Cancelled || room.Status == RoomStatus.Locked || room.Status == RoomStatus.InProgress || room.Status == RoomStatus.Reviewing)
            if (room.Status != RoomStatus.Open)

            {
                return Result.Failure<KickPlayerResponse>(MatchRoomErrors.RoomNotActive);
            }

            // 4. Check if host is trying to kick themselves
            if (request.TargetUserId == hostId)
            {
                return Result.Failure<KickPlayerResponse>(MatchRoomErrors.CannotKickSelf);
            }

            // 5. Find the target participant
            var targetParticipant = room.RoomParticipants.FirstOrDefault(p => p.UserId == request.TargetUserId);
            if (targetParticipant == null)
            {
                return Result.Failure<KickPlayerResponse>(MatchRoomErrors.PlayerNotInRoom(request.TargetUserId));
            }

            // Get user name before removal for notification
            var kickedUserName = targetParticipant.User?.FullName ?? "Unknown Player";

            try
            {
                // CAPTAIN LOGIC: Handle succession before removal
                if (targetParticipant.IsCaptain && targetParticipant.TeamAssignment != TeamAssignment.Unassigned)
                {
                    // Find heir for the team (exclude kicked user)
                    var newCaptainId = await _roomParticipantRepository.AssignNewCaptainAsync(
                        request.RoomId, targetParticipant.TeamAssignment, request.TargetUserId, cancellationToken);
                    
                    if (newCaptainId.HasValue)
                    {
                        _logger.LogInformation("Captain succession: User {OldCaptain} kicked from team {Team}, new captain is {NewCaptain}",
                            request.TargetUserId, targetParticipant.TeamAssignment, newCaptainId.Value);
                    }
                }

                // 6. Remove the participant
                _roomParticipantRepository.Remove(targetParticipant);

                // 7. Update room slots
                room.FilledSlots--;

                // 8. If room was Locked (full) and now has space, reopen it
                if (room.Status == RoomStatus.Locked && room.FilledSlots < room.TotalSlots)
                {
                    room.Status = RoomStatus.Open;
                    _logger.LogInformation("Room {RoomId} reopened after kicking player", request.RoomId);
                }

                // 9. Save changes
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("User {TargetUserId} was kicked from room {RoomId} by host {HostId}",
                    request.TargetUserId, request.RoomId, hostId);

                await _publisher.Publish(
                    new PlayerKickedFromMatchRoomDomainEvent(
                        request.RoomId,
                        request.TargetUserId,
                        room.RoomName,
                        null),
                    cancellationToken);

                // 10. Send real-time notifications
                await _matchRoomHubService.NotifyUserKickedAsync(
                    request.RoomId,
                    request.TargetUserId,
                    kickedUserName,
                    room.FilledSlots,
                    room.TotalSlots,
                    cancellationToken);

                return Result.Success(new KickPlayerResponse(
                    request.RoomId,
                    request.TargetUserId,
                    kickedUserName,
                    room.FilledSlots,
                    room.TotalSlots,
                    room.Status.ToString(),
                    $"Successfully kicked {kickedUserName} from the room"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to kick user {TargetUserId} from room {RoomId}",
                    request.TargetUserId, request.RoomId);
                return Result.Failure<KickPlayerResponse>(MatchRoomErrors.KickFailed);
            }
        }
    }
}
