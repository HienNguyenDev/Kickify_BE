using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Abstractions.Services;
using Kickify.Domain.Common;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Kickify.Application.Features.MatchRooms.Commands.JoinRoom
{
    public class JoinRoomCommandHandler : IRequestHandler<JoinRoomCommand, Result<JoinRoomResponse>>
    {
        private readonly IMatchRoomRepository _matchRoomRepository;
        private readonly IUserRepository _userRepository;
        private readonly IRoomParticipantRepository _roomParticipantRepository;
        private readonly IMatchRoomHubService _matchRoomHubService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<JoinRoomCommandHandler> _logger;

        public JoinRoomCommandHandler(
            IMatchRoomRepository matchRoomRepository,
            IUserRepository userRepository,
            IRoomParticipantRepository roomParticipantRepository,
            IMatchRoomHubService matchRoomHubService,
            IUnitOfWork unitOfWork,
            ILogger<JoinRoomCommandHandler> logger)
        {
            _matchRoomRepository = matchRoomRepository;
            _userRepository = userRepository;
            _roomParticipantRepository = roomParticipantRepository;
            _matchRoomHubService = matchRoomHubService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<JoinRoomResponse>> Handle(JoinRoomCommand request, CancellationToken cancellationToken)
        {
            // Verify user exists
            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user == null)
            {
                return Result.Failure<JoinRoomResponse>(UserErrors.NotFound(request.UserId));
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

            // RULE #4: Check if user is already in room
            if (room.RoomParticipants.Any(p => p.UserId == request.UserId))
            {
                return Result.Failure<JoinRoomResponse>(MatchRoomErrors.AlreadyJoined);
            }

            // RULE #4: Check if room is full (with concurrency safety)
            if (room.FilledSlots >= room.TotalSlots)
            {
                return Result.Failure<JoinRoomResponse>(MatchRoomErrors.RoomFull);
            }

            try
            {
                // Add participant
                var participant = new RoomParticipant
                {
                    ParticipantId = Guid.NewGuid(),
                    RoomId = request.RoomId,
                    UserId = request.UserId,
                    TeamAssignment = TeamAssignment.Unassigned,
                    JoinDate = DateTime.UtcNow,
                    DepositPaid = false,
                    DepositAmount = room.DepositPerPerson
                };

                // Add participant via repository
                await _roomParticipantRepository.AddAsync(participant);

                // RULE #4: Increment FilledSlots
                room.FilledSlots++;
                _matchRoomRepository.Update(room);

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("User {UserId} joined room {RoomId}. Filled: {FilledSlots}/{TotalSlots}",
                    request.UserId, request.RoomId, room.FilledSlots, room.TotalSlots);

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
                    request.UserId,
                    room.FilledSlots,
                    room.TotalSlots,
                    participant.JoinDate
                ));
            }
            catch (DbUpdateException ex)
            {
                // Handle potential concurrency conflict (multiple users joining simultaneously)
                _logger.LogWarning(ex, "Concurrency conflict when user {UserId} tried to join room {RoomId}",
                    request.UserId, request.RoomId);

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
