using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Abstractions.Services;
using Kickify.Domain.Common;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace Kickify.Application.Features.MatchRooms.Commands.RenameTeam
{
    public class RenameTeamCommandHandler : ICommandHandler<RenameTeamCommand, RenameTeamResponse>
    {
        private readonly IMatchRoomRepository _matchRoomRepository;
        private readonly IMatchRoomHubService _matchRoomHubService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserContext _userContext;
        private readonly ILogger<RenameTeamCommandHandler> _logger;

        public RenameTeamCommandHandler(
            IMatchRoomRepository matchRoomRepository,
            IMatchRoomHubService matchRoomHubService,
            IUnitOfWork unitOfWork,
            IUserContext userContext,
            ILogger<RenameTeamCommandHandler> logger)
        {
            _matchRoomRepository = matchRoomRepository;
            _matchRoomHubService = matchRoomHubService;
            _unitOfWork = unitOfWork;
            _userContext = userContext;
            _logger = logger;
        }

        public async Task<Result<RenameTeamResponse>> Handle(RenameTeamCommand request, CancellationToken cancellationToken)
        {
            var userId = _userContext.UserId;

            // Get room with participants
            var room = await _matchRoomRepository.GetRoomWithParticipantsAsync(request.RoomId, cancellationToken);
            if (room == null)
            {
                return Result.Failure<RenameTeamResponse>(MatchRoomErrors.NotFound(request.RoomId));
            }

            // Check if room is still active
            if (room.Status != RoomStatus.Open)
            {
                return Result.Failure<RenameTeamResponse>(MatchRoomErrors.RoomNotActive);
            }

            // Parse team assignment
            if (!Enum.TryParse<TeamAssignment>(request.Team, true, out var teamAssignment) ||
                teamAssignment == TeamAssignment.Unassigned)
            {
                return Result.Failure<RenameTeamResponse>(MatchRoomErrors.InvalidTeamForFormation);
            }

            // Check if user is a participant
            var userParticipant = room.RoomParticipants.FirstOrDefault(p => p.UserId == userId);
            if (userParticipant == null)
            {
                return Result.Failure<RenameTeamResponse>(MatchRoomErrors.NotParticipant);
            }

            // Check if user is a captain
            if (!userParticipant.IsCaptain)
            {
                return Result.Failure<RenameTeamResponse>(MatchRoomErrors.NotCaptain);
            }

            // Check if user is captain of the specified team
            if (userParticipant.TeamAssignment != teamAssignment)
            {
                return Result.Failure<RenameTeamResponse>(MatchRoomErrors.CannotUpdateOtherTeamName);
            }

            // Validate team name length
            if (request.Name != null && request.Name.Length > 50)
            {
                return Result.Failure<RenameTeamResponse>(MatchRoomErrors.TeamNameTooLong);
            }

            // Update team name based on team
            if (teamAssignment == TeamAssignment.A)
            {
                room.TeamAName = request.Name;
            }
            else
            {
                room.TeamBName = request.Name;
            }

            _matchRoomRepository.Update(room);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var updatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

            _logger.LogInformation(
                "Team name updated for Room {RoomId}, Team {Team} to '{TeamName}' by captain {UserId}",
                request.RoomId, request.Team, request.Name, userId);

            // Send real-time notification
            await _matchRoomHubService.NotifyTeamNameUpdatedAsync(
                request.RoomId,
                request.Team,
                request.Name,
                cancellationToken);

            return Result.Success(new RenameTeamResponse(
                request.RoomId,
                request.Team,
                request.Name,
                updatedAt
            ));
        }
    }
}
