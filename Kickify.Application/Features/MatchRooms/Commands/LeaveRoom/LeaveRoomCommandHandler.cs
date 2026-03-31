using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Abstractions.Services;
using Kickify.Domain.Common;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;
using Kickify.Domain.Event;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Kickify.Application.Features.MatchRooms.Commands.LeaveRoom
{
    public class LeaveRoomCommandHandler : ICommandHandler<LeaveRoomCommand, LeaveRoomResponse>
    {
        private readonly IMatchRoomRepository _matchRoomRepository;
        private readonly IUserRepository _userRepository;
        private readonly IRoomParticipantRepository _roomParticipantRepository;
        private readonly IChatMessageRepository _chatMessageRepository;
        private readonly IMatchRoomHubService _matchRoomHubService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserContext _userContext;
        private readonly IPublisher _publisher;
        private readonly ILogger<LeaveRoomCommandHandler> _logger;

        public LeaveRoomCommandHandler(
            IMatchRoomRepository matchRoomRepository,
            IUserRepository userRepository,
            IRoomParticipantRepository roomParticipantRepository,
            IChatMessageRepository chatMessageRepository,
            IMatchRoomHubService matchRoomHubService,
            IUnitOfWork unitOfWork,
            IUserContext userContext,
            IPublisher publisher,
            ILogger<LeaveRoomCommandHandler> logger)
        {
            _matchRoomRepository = matchRoomRepository;
            _userRepository = userRepository;
            _roomParticipantRepository = roomParticipantRepository;
            _chatMessageRepository = chatMessageRepository;
            _matchRoomHubService = matchRoomHubService;
            _unitOfWork = unitOfWork;
            _userContext = userContext;
            _publisher = publisher;
            _logger = logger;
        }

        public async Task<Result<LeaveRoomResponse>> Handle(LeaveRoomCommand request, CancellationToken cancellationToken)
        {
            var userId = _userContext.UserId;
            
            // Verify user exists
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return Result.Failure<LeaveRoomResponse>(UserErrors.NotFound(userId));
            }

            // Get room with participants (WITH TRACKING for update/delete)
            var room = await _matchRoomRepository.GetRoomWithParticipantsForUpdateAsync(request.RoomId, cancellationToken);
            if (room == null)
            {
                return Result.Failure<LeaveRoomResponse>(MatchRoomErrors.NotFound(request.RoomId));
            }

            // Find participant
            var participant = room.RoomParticipants.FirstOrDefault(p => p.UserId == userId);
            if (participant == null)
            {
                return Result.Failure<LeaveRoomResponse>(MatchRoomErrors.NotParticipant);
            }

            try
            {
                // CAPTAIN LOGIC: Handle succession before removal
                if (participant.IsCaptain && participant.TeamAssignment != TeamAssignment.Unassigned)
                {
                    // Find heir for the team (exclude current user since they're leaving)
                    var newCaptainId = await _roomParticipantRepository.AssignNewCaptainAsync(
                        request.RoomId, participant.TeamAssignment, userId, cancellationToken);
                    
                    if (newCaptainId.HasValue)
                    {
                        _logger.LogInformation("Captain succession: User {OldCaptain} leaving team {Team}, new captain is {NewCaptain}",
                            userId, participant.TeamAssignment, newCaptainId.Value);
                    }
                }

                // RULE #5: Check if user is host
                bool isHost = room.HostId == userId;
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
                        request.RoomId, userId);
                }
                else if (isHost && room.FilledSlots > 1)
                {
                    // Host leaves but others remain - reassign host to first non-host participant
                    var newHost = room.RoomParticipants.FirstOrDefault(p => p.UserId != userId);
                    if (newHost != null)
                    {
                        room.HostId = newHost.UserId;
                        newHostId = newHost.UserId;
                        _logger.LogInformation("Room {RoomId} host reassigned from {OldHostId} to {NewHostId}",
                            request.RoomId, userId, newHost.UserId);
                    }

                    // RULE: When host leaves a Private room, reset to Public and clear password
                    if (room.Visibility == Visibility.Private)
                    {
                        room.Visibility = Visibility.Public;
                        room.RoomPassword = null;

                        _logger.LogInformation("Room {RoomId} privacy reset to Public after host {HostId} left",
                            request.RoomId, userId);

                        // Create system chat message about privacy reset
                        var systemMessage = new ChatMessage
                        {
                            MessageId = Guid.NewGuid(),
                            RoomId = room.RoomId,
                            SenderId = room.HostId, // Use new host as sender for system message
                            ConversationType = ConversationType.Room,
                            RoomChatChannel = RoomChatChannel.General,
                            MessageText = "Room privacy has been reset to Public because the previous host left.",
                            MessageType = MessageType.System,
                            SentAt = DateTime.UtcNow // ChatMessage uses 'timestamp with time zone' - requires UTC
                        };
                        await _chatMessageRepository.AddAsync(systemMessage);

                        // Notify about privacy change
                        await _matchRoomHubService.NotifyRoomPrivacyUpdatedAsync(
                            room.RoomId,
                            Visibility.Public.ToString(),
                            false,
                            cancellationToken);
                    }

                    // RULE: Subtract participant's deposit from TotalDepositCollected
                    if (participant.DepositPaid && participant.DepositAmount.HasValue)
                    {
                        room.TotalDepositCollected -= participant.DepositAmount.Value;
                        _logger.LogInformation("Subtracted deposit {Deposit} from room {RoomId}. New total: {TotalDeposit}",
                            participant.DepositAmount.Value, request.RoomId, room.TotalDepositCollected);
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
                    
                    // RULE: Subtract participant's deposit from TotalDepositCollected
                    if (participant.DepositPaid && participant.DepositAmount.HasValue)
                    {
                        room.TotalDepositCollected -= participant.DepositAmount.Value;
                        _logger.LogInformation("Subtracted deposit {Deposit} from room {RoomId}. New total: {TotalDeposit}",
                            participant.DepositAmount.Value, request.RoomId, room.TotalDepositCollected);
                    }

                    _roomParticipantRepository.Remove(participant);
                    room.FilledSlots--;
                    _matchRoomRepository.Update(room);
                    message = "You left the room successfully";

                    _logger.LogInformation("User {UserId} left room {RoomId}. Filled: {FilledSlots}/{TotalSlots}",
                        userId, request.RoomId, room.FilledSlots, room.TotalSlots);
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                if (!isRoomDeleted)
                {
                    await _publisher.Publish(
                        new MatchRoomPlayerLeftHostNotifyDomainEvent(
                            room.RoomId,
                            room.HostId,
                            user.FullName ?? user.Email,
                            room.RoomName),
                        cancellationToken);
                }

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
                    userId,
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
