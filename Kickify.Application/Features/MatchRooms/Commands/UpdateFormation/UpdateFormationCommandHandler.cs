using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Abstractions.Services;
using Kickify.Application.Features.MatchRooms.Services;
using Kickify.Domain.Common;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace Kickify.Application.Features.MatchRooms.Commands.UpdateFormation
{
    public class UpdateFormationCommandHandler : ICommandHandler<UpdateFormationCommand, UpdateFormationResponse>
    {
        private readonly IMatchRoomRepository _matchRoomRepository;
        private readonly IRoomParticipantRepository _roomParticipantRepository;
        private readonly IMatchFormationRepository _matchFormationRepository;
        private readonly IFormationAssignmentRepository _formationAssignmentRepository;
        private readonly IMatchRoomHubService _matchRoomHubService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserContext _userContext;
        private readonly ILogger<UpdateFormationCommandHandler> _logger;

        public UpdateFormationCommandHandler(
            IMatchRoomRepository matchRoomRepository,
            IRoomParticipantRepository roomParticipantRepository,
            IMatchFormationRepository matchFormationRepository,
            IFormationAssignmentRepository formationAssignmentRepository,
            IMatchRoomHubService matchRoomHubService,
            IUnitOfWork unitOfWork,
            IUserContext userContext,
            ILogger<UpdateFormationCommandHandler> logger)
        {
            _matchRoomRepository = matchRoomRepository;
            _roomParticipantRepository = roomParticipantRepository;
            _matchFormationRepository = matchFormationRepository;
            _formationAssignmentRepository = formationAssignmentRepository;
            _matchRoomHubService = matchRoomHubService;
            _unitOfWork = unitOfWork;
            _userContext = userContext;
            _logger = logger;
        }

        public async Task<Result<UpdateFormationResponse>> Handle(UpdateFormationCommand request, CancellationToken cancellationToken)
        {
            var userId = _userContext.UserId;

            // Get room with participants
            var room = await _matchRoomRepository.GetRoomWithParticipantsAsync(request.RoomId, cancellationToken);
            if (room == null)
            {
                return Result.Failure<UpdateFormationResponse>(MatchRoomErrors.NotFound(request.RoomId));
            }

            //// Check if room is still active
            //if (room.Status != RoomStatus.Open)
            //{
            //    return Result.Failure<UpdateFormationResponse>(MatchRoomErrors.RoomNotActive);
            //}

            // Parse team assignment
            if (!Enum.TryParse<TeamAssignment>(request.Team, true, out var teamAssignment) ||
                teamAssignment == TeamAssignment.Unassigned)
            {
                return Result.Failure<UpdateFormationResponse>(MatchRoomErrors.InvalidTeamForFormation);
            }

            // Check if user is captain of the specified team
            var userParticipant = room.RoomParticipants.FirstOrDefault(p => p.UserId == userId);
            if (userParticipant == null)
            {
                return Result.Failure<UpdateFormationResponse>(MatchRoomErrors.NotParticipant);
            }

            if (!userParticipant.IsCaptain || userParticipant.TeamAssignment != teamAssignment)
            {
                return Result.Failure<UpdateFormationResponse>(MatchRoomErrors.NotCaptain);
            }

            // Validate formation name for the match format
            if (!FormationRuleService.IsValidFormation(room.MatchFormat, request.FormationName))
            {
                return Result.Failure<UpdateFormationResponse>(
                    MatchRoomErrors.InvalidFormation(request.FormationName, room.MatchFormat.ToString()));
            }

            // Get expected slots for the formation
            var expectedSlots = FormationRuleService.GenerateExpectedSlots(room.MatchFormat, request.FormationName);

            // Validate assignments
            var teamPlayers = room.RoomParticipants
                .Where(p => p.TeamAssignment == teamAssignment)
                .ToDictionary(p => p.UserId, p => p);

            var assignedSlots = new HashSet<string>();
            var assignedPlayers = new HashSet<Guid>();

            foreach (var assignment in request.Assignments)
            {
                // Check if slot is valid for this formation
                if (!expectedSlots.Contains(assignment.SlotId))
                {
                    return Result.Failure<UpdateFormationResponse>(
                        MatchRoomErrors.InvalidSlotId(assignment.SlotId));
                }

                // Check for duplicate slot assignment
                if (!assignedSlots.Add(assignment.SlotId))
                {
                    return Result.Failure<UpdateFormationResponse>(
                        MatchRoomErrors.DuplicateSlotAssignment(assignment.SlotId));
                }

                // Check for duplicate player assignment
                if (!assignedPlayers.Add(assignment.PlayerId))
                {
                    return Result.Failure<UpdateFormationResponse>(
                        MatchRoomErrors.DuplicatePlayerAssignment(assignment.PlayerId));
                }

                // Check if player is on the team
                if (!teamPlayers.ContainsKey(assignment.PlayerId))
                {
                    return Result.Failure<UpdateFormationResponse>(
                        MatchRoomErrors.PlayerNotOnTeam(assignment.PlayerId));
                }
            }

            // Get or create formation for this team
            var existingFormation = await _matchFormationRepository.GetFormationByRoomAndTeamAsync(
                request.RoomId, teamAssignment, cancellationToken);

            MatchFormation formation;
            if (existingFormation != null)
            {
                // Update existing formation
                formation = existingFormation;
                formation.FormationName = request.FormationName;
                formation.MatchFormat = room.MatchFormat.ToString();

                // Delete existing assignments
                await _formationAssignmentRepository.DeleteByFormationIdAsync(formation.FormationId, cancellationToken);
                _matchFormationRepository.Update(formation);
            }
            else
            {
                // Create new formation
                formation = new MatchFormation
                {
                    FormationId = Guid.NewGuid(),
                    RoomId = request.RoomId,
                    TeamAssignment = teamAssignment,
                    FormationName = request.FormationName,
                    MatchFormat = room.MatchFormat.ToString()
                };
                await _matchFormationRepository.AddAsync(formation);
            }

            // Create new assignments and update participant positions
            var assignmentResponses = new List<FormationSlotResponse>();
            foreach (var assignment in request.Assignments)
            {
                var formationAssignment = new FormationAssignment
                {
                    AssignmentId = Guid.NewGuid(),
                    FormationId = formation.FormationId,
                    PlayerId = assignment.PlayerId,
                    SlotId = assignment.SlotId
                };
                await _formationAssignmentRepository.AddAsync(formationAssignment);

                // Update RoomParticipant.Position
                var participant = teamPlayers[assignment.PlayerId];
                participant.Position = FormationRuleService.GetPositionFromSlotId(assignment.SlotId);
                _roomParticipantRepository.Update(participant);

                // Build response
                assignmentResponses.Add(new FormationSlotResponse(
                    assignment.PlayerId,
                    participant.User?.FullName ?? "Unknown",
                    assignment.SlotId,
                    participant.Position
                ));
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var updatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

            _logger.LogInformation(
                "Formation updated for Room {RoomId}, Team {Team} to {FormationName} by captain {UserId}",
                request.RoomId, request.Team, request.FormationName, userId);

            var response = new UpdateFormationResponse(
                request.RoomId,
                request.Team,
                request.FormationName,
                assignmentResponses,
                updatedAt
            );

            // Send real-time notification with exact same structure as API response
            await _matchRoomHubService.NotifyFormationUpdatedAsync(response, cancellationToken);

            return Result.Success(response);
        }
    }
}
