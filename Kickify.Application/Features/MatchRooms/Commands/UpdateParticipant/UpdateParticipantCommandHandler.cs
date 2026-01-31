using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Abstractions.Services;
using Kickify.Domain.Common;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace Kickify.Application.Features.MatchRooms.Commands.UpdateParticipant
{
    public class UpdateParticipantCommandHandler : ICommandHandler<UpdateParticipantCommand, UpdateParticipantResponse>
    {
        private readonly IMatchRoomRepository _matchRoomRepository;
        private readonly IUserRepository _userRepository;
        private readonly IRoomParticipantRepository _roomParticipantRepository;
        private readonly IMatchRoomHubService _matchRoomHubService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserContext _userContext;
        private readonly ILogger<UpdateParticipantCommandHandler> _logger;

        public UpdateParticipantCommandHandler(
            IMatchRoomRepository matchRoomRepository,
            IUserRepository userRepository,
            IRoomParticipantRepository roomParticipantRepository,
            IMatchRoomHubService matchRoomHubService,
            IUnitOfWork unitOfWork,
            IUserContext userContext,
            ILogger<UpdateParticipantCommandHandler> logger)
        {
            _matchRoomRepository = matchRoomRepository;
            _userRepository = userRepository;
            _roomParticipantRepository = roomParticipantRepository;
            _matchRoomHubService = matchRoomHubService;
            _unitOfWork = unitOfWork;
            _userContext = userContext;
            _logger = logger;
        }

        public async Task<Result<UpdateParticipantResponse>> Handle(UpdateParticipantCommand request, CancellationToken cancellationToken)
        {
            var userId = _userContext.UserId;
            
            // Verify user exists
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return Result.Failure<UpdateParticipantResponse>(UserErrors.NotFound(userId));
            }

            // Get room with participants
            var room = await _matchRoomRepository.GetRoomWithParticipantsAsync(request.RoomId, cancellationToken);
            if (room == null)
            {
                return Result.Failure<UpdateParticipantResponse>(MatchRoomErrors.NotFound(request.RoomId));
            }

            // Find participant
            var participant = room.RoomParticipants.FirstOrDefault(p => p.UserId == userId);
            if (participant == null)
            {
                return Result.Failure<UpdateParticipantResponse>(MatchRoomErrors.NotParticipant);
            }

            try
            {
                // Update team assignment if provided
                if (!string.IsNullOrEmpty(request.TeamAssignment))
                {
                    if (Enum.TryParse<TeamAssignment>(request.TeamAssignment, true, out var teamAssignment))
                    {
                        participant.TeamAssignment = teamAssignment;
                    }
                    else
                    {
                        return Result.Failure<UpdateParticipantResponse>(MatchRoomErrors.InvalidTeam(request.TeamAssignment));
                    }
                }

                // Update position if provided
                if (request.Position != null)
                {
                    participant.Position = request.Position;
                }

                // RULE: Track when participant was last updated (team/position change)
                participant.UpdatedAt = DateTime.UtcNow;

                // Mark participant as modified
                _roomParticipantRepository.Update(participant);

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Send real-time notification to all room participants
                await _matchRoomHubService.NotifyParticipantUpdatedAsync(
                    room.RoomId,
                    user.UserId,
                    user.FullName ?? user.Email,
                    user.AvatarUrl,
                    participant.TeamAssignment.ToString(),
                    participant.Position,
                    cancellationToken);

                _logger.LogInformation("Participant {UserId} updated in room {RoomId}. Team: {Team}, Position: {Position}",
                    userId, request.RoomId, participant.TeamAssignment, participant.Position);

                return Result.Success(new UpdateParticipantResponse(
                    participant.ParticipantId,
                    room.RoomId,
                    userId,
                    participant.TeamAssignment.ToString(),
                    participant.Position,
                    participant.UpdatedAt ?? DateTime.UtcNow
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating participant");
                throw;
            }
        }
    }
}
