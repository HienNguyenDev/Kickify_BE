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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Kickify.Application.Features.MatchRooms.Commands.JoinRoom
{
    public class JoinRoomCommandHandler : ICommandHandler<JoinRoomCommand, JoinRoomResponse>
    {
        private readonly IMatchRoomRepository _matchRoomRepository;
        private readonly IUserRepository _userRepository;
        private readonly IRoomParticipantRepository _roomParticipantRepository;
        private readonly IMatchRoomHubService _matchRoomHubService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserContext _userContext;
        private readonly ILogger<JoinRoomCommandHandler> _logger;

        public JoinRoomCommandHandler(
            IMatchRoomRepository matchRoomRepository,
            IUserRepository userRepository,
            IRoomParticipantRepository roomParticipantRepository,
            IMatchRoomHubService matchRoomHubService,
            IUnitOfWork unitOfWork,
            IUserContext userContext,
            ILogger<JoinRoomCommandHandler> logger)
        {
            _matchRoomRepository = matchRoomRepository;
            _userRepository = userRepository;
            _roomParticipantRepository = roomParticipantRepository;
            _matchRoomHubService = matchRoomHubService;
            _unitOfWork = unitOfWork;
            _userContext = userContext;
            _logger = logger;
        }

        public async Task<Result<JoinRoomResponse>> Handle(JoinRoomCommand request, CancellationToken cancellationToken)
        {
            var userId = _userContext.UserId;
            
            // Verify user exists
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return Result.Failure<JoinRoomResponse>(UserErrors.NotFound(userId));
            }

            // Check if user is already in this room
            var existingParticipant = await _roomParticipantRepository.GetParticipantByRoomAndUserAsync(request.RoomId, userId, cancellationToken);
            if (existingParticipant != null)
            {
                // User already in room, get room info and return success
                var existingRoom = await _matchRoomRepository.GetByIdAsync(request.RoomId);
                if (existingRoom == null)
                {
                    return Result.Failure<JoinRoomResponse>(MatchRoomErrors.NotFound(request.RoomId));
                }

                _logger.LogInformation("User {UserId} is already in room {RoomId}. Returning existing participant info.",
                    userId, request.RoomId);

                return Result.Success(new JoinRoomResponse(
                    existingParticipant.ParticipantId,
                    existingRoom.RoomId,
                    userId,
                    existingRoom.FilledSlots,
                    existingRoom.TotalSlots,
                    existingParticipant.JoinDate
                ));
            }

            // Get room with participants (WITH TRACKING for FilledSlots update)
            var room = await _matchRoomRepository.GetRoomWithParticipantsForUpdateAsync(request.RoomId, cancellationToken);
            if (room == null)
            {
                return Result.Failure<JoinRoomResponse>(MatchRoomErrors.NotFound(request.RoomId));
            }

            // Check if room is open
            if (room.Status != RoomStatus.Open)
            {
                return Result.Failure<JoinRoomResponse>(MatchRoomErrors.NotOpen);
            }

            var targetEndTime = room.StartTime.Add(TimeSpan.FromMinutes(room.DurationMinutes));

            // This query: Get all MatchRoomParticipent of User (Participant) + came along with MatchDate + Status is Open/Locked/InProgress
            var userActiveRooms = await _matchRoomRepository.GetActiveRoomsForUserByDateAsync(userId, room.MatchDate, cancellationToken);

            foreach (var activeRoom in userActiveRooms)
            {
                // Bo qua chinh cái phong dang dinh join (phong ho case logic bi lap)
                if (activeRoom.RoomId == room.RoomId) continue;

                var activeEndTime = activeRoom.StartTime.Add(TimeSpan.FromMinutes(activeRoom.DurationMinutes));

                // Thuat toan kiem tra 2 khoang thoi gian co giao nhau khong (Overlapping)
                bool isOverlapping = room.StartTime < activeEndTime && targetEndTime > activeRoom.StartTime;

                if (isOverlapping)
                {
                    _logger.LogWarning("User {UserId} attempted to join room {RoomId} but has a time conflict with room {ActiveRoomId}",
                        userId, room.RoomId, activeRoom.RoomId);

                    return Result.Failure<JoinRoomResponse>(MatchRoomErrors.TimeConflict(activeRoom.RoomName));
                }
            }

            // RULE: Validate password for private rooms
            if (room.Visibility == Visibility.Private)
            {
                if (string.IsNullOrEmpty(request.Password))
                {
                    return Result.Failure<JoinRoomResponse>(MatchRoomErrors.PasswordRequiredForPrivateRoom);
                }

                if (room.RoomPassword != request.Password)
                {
                    return Result.Failure<JoinRoomResponse>(MatchRoomErrors.IncorrectRoomPassword);
                }
            }

            // RULE #4: Check if room is full (with concurrency safety)
            if (room.FilledSlots >= room.TotalSlots)
            {
                return Result.Failure<JoinRoomResponse>(MatchRoomErrors.RoomFull);
            }

            try
            {
                // Add participant
                // RULE: New participants join as Unassigned with IsCaptain = false (default)
                // They will become captain when they switch to Team A/B if that team has no captain
                var participant = new RoomParticipant
                {
                    ParticipantId = Guid.NewGuid(),
                    RoomId = request.RoomId,
                    UserId = userId,
                    TeamAssignment = TeamAssignment.Unassigned,
                    JoinDate = DateTime.UtcNow,
                    DepositPaid = false,
                    DepositAmount = room.DepositPerPerson,
                    IsCaptain = false // Explicitly set - Unassigned players cannot be captains
                };

                // Add participant via repository
                await _roomParticipantRepository.AddAsync(participant);

            var oldJobId = room.AutoCloseJobId;
                room.FilledSlots++;
            room.Raise(new ParticipantJoinedRoomDomainEvent(room.RoomId, oldJobId));
                _matchRoomRepository.Update(room);

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("User {UserId} joined room {RoomId}. Filled: {FilledSlots}/{TotalSlots}",
                    userId, request.RoomId, room.FilledSlots, room.TotalSlots);

                // Send real-time notification to all room participants
                await _matchRoomHubService.NotifyUserJoinedAsync(
                    room.RoomId,
                    user.UserId,
                    user.FullName ?? user.Email,
                    user.AvatarUrl,
                    room.FilledSlots,
                    room.TotalSlots,
                    cancellationToken);

                return Result.Success(new JoinRoomResponse(
                    participant.ParticipantId,
                    room.RoomId,
                    userId,
                    room.FilledSlots,
                    room.TotalSlots,
                    participant.JoinDate
                ));
            }
            catch (DbUpdateException ex)
            {
                // Handle potential concurrency conflict (multiple users joining simultaneously)
                _logger.LogWarning(ex, "Concurrency conflict when user {UserId} tried to join room {RoomId}",
                    userId, request.RoomId);

                return Result.Failure<JoinRoomResponse>(
                    new Error("MatchRoom.ConcurrencyConflict", "Room was just filled by another user. Please try another room.", ErrorType.Conflict));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error joining room");
                throw;
            }
        }
    }
}
