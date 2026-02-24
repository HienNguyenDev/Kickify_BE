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

            // Get room with participants (WITH TRACKING for update)
            var room = await _matchRoomRepository.GetRoomWithParticipantsForUpdateAsync(request.RoomId, cancellationToken);
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
                // Capture old state for captain succession logic
                var wasCaptain = participant.IsCaptain;
                var oldTeam = participant.TeamAssignment;
                TeamAssignment newTeam = oldTeam;

                // Update team assignment if provided
                if (!string.IsNullOrEmpty(request.TeamAssignment))
                {
                    if (Enum.TryParse<TeamAssignment>(request.TeamAssignment, true, out var parsedTeam))
                    {
                        newTeam = parsedTeam;
                    }
                    else
                    {
                        return Result.Failure<UpdateParticipantResponse>(MatchRoomErrors.InvalidTeam(request.TeamAssignment));
                    }
                }

                // CAPTAIN LOGIC: Handle team change
                if (newTeam != oldTeam)
                {
                    // Step 1: Handle Old Team Succession (if was captain and leaving a real team)
                    if (wasCaptain && oldTeam != TeamAssignment.Unassigned)
                    {
                        participant.IsCaptain = false;
                        
                        // Find heir for old team (exclude current user)
                        var newCaptainId = await _roomParticipantRepository.AssignNewCaptainAsync(
                            request.RoomId, oldTeam, userId, cancellationToken);
                        
                        if (newCaptainId.HasValue)
                        {
                            _logger.LogInformation("Captain succession: User {OldCaptain} left team {Team}, new captain is {NewCaptain}",
                                userId, oldTeam, newCaptainId.Value);
                        }
                    }

                    // Step 2: Update team assignment
                    participant.TeamAssignment = newTeam;

                    // Step 3: Handle New Team Captain Assignment
                    if (newTeam != TeamAssignment.Unassigned)
                    {
                        // Check if new team has a captain
                        var hasNewTeamCaptain = await _roomParticipantRepository.HasTeamCaptainAsync(
                            request.RoomId, newTeam, cancellationToken);
                        
                        if (!hasNewTeamCaptain)
                        {
                            // No captain in new team - this user becomes captain
                            participant.IsCaptain = true;
                            _logger.LogInformation("User {UserId} became captain of team {Team} (first to join)",
                                userId, newTeam);
                        }
                    }
                    else
                    {
                        // Moving to Unassigned - cannot be captain
                        participant.IsCaptain = false;
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

                _logger.LogInformation("Participant {UserId} updated in room {RoomId}. Team: {Team}, Position: {Position}, IsCaptain: {IsCaptain}",
                    userId, request.RoomId, participant.TeamAssignment, participant.Position, participant.IsCaptain);

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
