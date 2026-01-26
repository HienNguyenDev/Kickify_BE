using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Abstractions.Services;
using Kickify.Domain.Common;
using Kickify.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace Kickify.Application.Features.MatchRooms.Commands.LeaveRoom
{
    public class LeaveRoomCommandHandler : ICommandHandler<LeaveRoomCommand, LeaveRoomResponse>
    {
        private readonly IMatchRoomRepository _matchRoomRepository;
        private readonly IUserRepository _userRepository;
        private readonly IRoomParticipantRepository _roomParticipantRepository;
        private readonly IMatchRoomHubService _matchRoomHubService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<LeaveRoomCommandHandler> _logger;

        public LeaveRoomCommandHandler(
            IMatchRoomRepository matchRoomRepository,
            IUserRepository userRepository,
            IRoomParticipantRepository roomParticipantRepository,
            IMatchRoomHubService matchRoomHubService,
            IUnitOfWork unitOfWork,
            ILogger<LeaveRoomCommandHandler> logger)
        {
            _matchRoomRepository = matchRoomRepository;
            _userRepository = userRepository;
            _roomParticipantRepository = roomParticipantRepository;
            _matchRoomHubService = matchRoomHubService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<LeaveRoomResponse>> Handle(LeaveRoomCommand request, CancellationToken cancellationToken)
        {
            // Verify user exists
            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user == null)
            {
                return Result.Failure<LeaveRoomResponse>(UserErrors.NotFound(request.UserId));
            }

            // Get room with participants (WITH TRACKING for update/delete)
            var room = await _matchRoomRepository.GetRoomWithParticipantsForUpdateAsync(request.RoomId, cancellationToken);
            if (room == null)
            {
                return Result.Failure<LeaveRoomResponse>(MatchRoomErrors.NotFound(request.RoomId));
            }

            // Find participant
            var participant = room.RoomParticipants.FirstOrDefault(p => p.UserId == request.UserId);
            if (participant == null)
            {
                return Result.Failure<LeaveRoomResponse>(MatchRoomErrors.NotParticipant);
            }

            try
            {
                // RULE #5: Check if user is host
                bool isHost = room.HostId == request.UserId;
                bool isRoomDeleted = false;
                Guid? newHostId = null;
                string message;

                if (isHost && room.FilledSlots == 1)
                {
                    // Host is the only one left - delete the room
                    isRoomDeleted = true;
                    _matchRoomRepository.Remove(room);
                    message = "Room deleted as you were the last participant";

                    _logger.LogInformation("Room {RoomId} deleted as host {UserId} was the last participant",
                        request.RoomId, request.UserId);
                }
                else if (isHost && room.FilledSlots > 1)
                {
                    // Host leaves but others remain - reassign host to first non-host participant
                    var newHost = room.RoomParticipants.FirstOrDefault(p => p.UserId != request.UserId);
                    if (newHost != null)
                    {
                        room.HostId = newHost.UserId;
                        newHostId = newHost.UserId;
                        _logger.LogInformation("Room {RoomId} host reassigned from {OldHostId} to {NewHostId}",
                            request.RoomId, request.UserId, newHost.UserId);
                    }

                    // Remove participant
                    _roomParticipantRepository.Remove(participant);
                    room.FilledSlots--;
                    _matchRoomRepository.Update(room);
                    message = "You left the room. Host role transferred to another participant";
                }
                else
                {
                    // Regular participant leaves
                    _roomParticipantRepository.Remove(participant);
                    room.FilledSlots--;
                    _matchRoomRepository.Update(room);
                    message = "You left the room successfully";

                    _logger.LogInformation("User {UserId} left room {RoomId}. Filled: {FilledSlots}/{TotalSlots}",
                        request.UserId, request.RoomId, room.FilledSlots, room.TotalSlots);
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Send real-time notification to all room participants
                await _matchRoomHubService.NotifyUserLeftAsync(
                    room.RoomId,
                    user.UserId,
                    user.FullName ?? user.Email,
                    room.FilledSlots,
                    room.TotalSlots,
                    isRoomDeleted,
                    newHostId,
                    cancellationToken);

                return Result.Success(new LeaveRoomResponse(
                    room.RoomId,
                    request.UserId,
                    room.FilledSlots,
                    room.TotalSlots,
                    message
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error leaving room");
                throw;
            }
        }
    }
}
