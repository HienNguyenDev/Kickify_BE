using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Kickify.Application.Features.MatchRooms.Commands.UpdateParticipant
{
    public class UpdateParticipantCommandHandler : IRequestHandler<UpdateParticipantCommand, Result<UpdateParticipantResponse>>
    {
        private readonly IMatchRoomRepository _matchRoomRepository;
        private readonly IUserRepository _userRepository;
        private readonly IRoomParticipantRepository _roomParticipantRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdateParticipantCommandHandler> _logger;

        public UpdateParticipantCommandHandler(
            IMatchRoomRepository matchRoomRepository,
            IUserRepository userRepository,
            IRoomParticipantRepository roomParticipantRepository,
            IUnitOfWork unitOfWork,
            ILogger<UpdateParticipantCommandHandler> logger)
        {
            _matchRoomRepository = matchRoomRepository;
            _userRepository = userRepository;
            _roomParticipantRepository = roomParticipantRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<UpdateParticipantResponse>> Handle(UpdateParticipantCommand request, CancellationToken cancellationToken)
        {
            // Verify user exists
            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user == null)
            {
                return Result.Failure<UpdateParticipantResponse>(UserErrors.NotFound(request.UserId));
            }

            // Get room with participants
            var room = await _matchRoomRepository.GetRoomWithParticipantsAsync(request.RoomId, cancellationToken);
            if (room == null)
            {
                return Result.Failure<UpdateParticipantResponse>(
                    new Error("MatchRoom.NotFound", $"Room with ID {request.RoomId} not found", ErrorType.NotFound));
            }

            // Find participant
            var participant = room.RoomParticipants.FirstOrDefault(p => p.UserId == request.UserId);
            if (participant == null)
            {
                return Result.Failure<UpdateParticipantResponse>(
                    new Error("MatchRoom.NotParticipant", "User is not a participant of this room", ErrorType.NotFound));
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
                        return Result.Failure<UpdateParticipantResponse>(
                            new Error("MatchRoom.InvalidTeam", $"Invalid team assignment: {request.TeamAssignment}", ErrorType.Validation));
                    }
                }

                // Update position if provided
                if (request.Position != null)
                {
                    participant.Position = request.Position;
                }

                // Mark participant as modified
                _roomParticipantRepository.Update(participant);

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Participant {UserId} updated in room {RoomId}. Team: {Team}, Position: {Position}",
                    request.UserId, request.RoomId, participant.TeamAssignment, participant.Position);

                return Result.Success(new UpdateParticipantResponse(
                    participant.ParticipantId,
                    room.RoomId,
                    request.UserId,
                    participant.TeamAssignment.ToString(),
                    participant.Position,
                    DateTime.UtcNow
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
