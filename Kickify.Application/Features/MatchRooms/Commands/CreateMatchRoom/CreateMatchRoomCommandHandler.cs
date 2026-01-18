using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Kickify.Application.Features.MatchRooms.Commands.CreateMatchRoom
{
    public class CreateMatchRoomCommandHandler : IRequestHandler<CreateMatchRoomCommand, Result<CreateMatchRoomResponse>>
    {
        private readonly IMatchRoomRepository _matchRoomRepository;
        private readonly IFieldRepository _fieldRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CreateMatchRoomCommandHandler> _logger;

        public CreateMatchRoomCommandHandler(
            IMatchRoomRepository matchRoomRepository,
            IFieldRepository fieldRepository,
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            ILogger<CreateMatchRoomCommandHandler> logger)
        {
            _matchRoomRepository = matchRoomRepository;
            _fieldRepository = fieldRepository;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<CreateMatchRoomResponse>> Handle(CreateMatchRoomCommand request, CancellationToken cancellationToken)
        {
            // Verify user exists
            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user == null)
            {
                return Result.Failure<CreateMatchRoomResponse>(UserErrors.NotFound(request.UserId));
            }

            // Verify field exists
            var field = await _fieldRepository.GetByIdAsync(request.FieldId);
            if (field == null)
            {
                return Result.Failure<CreateMatchRoomResponse>(FieldErrors.NotFound(request.FieldId));
            }

            // Parse MatchFormat enum
            if (!Enum.TryParse<MatchFormat>(request.MatchFormat, true, out var matchFormat))
            {
                return Result.Failure<CreateMatchRoomResponse>(
                    new Error("MatchRoom.InvalidFormat", $"Invalid match format: {request.MatchFormat}", ErrorType.Validation));
            }

            // RULE #1: Auto-calculate TotalSlots based on MatchFormat
            int totalSlots = CalculateTotalSlots(matchFormat);

            // RULE #2: Auto-calculate EndTime
            var endTime = request.StartTime.Add(TimeSpan.FromMinutes(request.DurationMinutes));

            try
            {
                // Create Match Room
                var room = new MatchRoom
                {
                    RoomId = Guid.NewGuid(),
                    HostId = request.UserId,
                    FieldId = request.FieldId,
                    MatchDate = request.MatchDate,
                    StartTime = request.StartTime,
                    DurationMinutes = request.DurationMinutes,
                    MatchFormat = matchFormat,
                    TotalSlots = totalSlots,
                    FilledSlots = 1, // RULE #3: Host is first participant
                    Description = request.Description,
                    Rules = request.Rules,
                    DepositPerPerson = request.DepositPerPerson,
                    Status = RoomStatus.Open,
                    CreatedAt = DateTime.UtcNow
                };

                await _matchRoomRepository.AddAsync(room);

                // RULE #3: Auto-add Host as first participant
                var hostParticipant = new RoomParticipant
                {
                    ParticipantId = Guid.NewGuid(),
                    RoomId = room.RoomId,
                    UserId = request.UserId,
                    TeamAssignment = TeamAssignment.Unassigned,
                    JoinDate = DateTime.UtcNow,
                    DepositPaid = false,
                    DepositAmount = request.DepositPerPerson
                };

                room.RoomParticipants.Add(hostParticipant);

                // Save all changes in transaction
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Match room {RoomId} created by user {UserId} with {TotalSlots} slots",
                    room.RoomId, request.UserId, totalSlots);

                return Result.Success(new CreateMatchRoomResponse(
                    room.RoomId,
                    room.HostId,
                    room.FieldId,
                    room.MatchDate,
                    room.StartTime,
                    endTime,
                    room.DurationMinutes,
                    room.MatchFormat.ToString(),
                    room.TotalSlots,
                    room.FilledSlots,
                    room.Status.ToString(),
                    room.CreatedAt
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating match room");
                throw;
            }
        }

        /// <summary>
        /// RULE #1: Calculate total slots based on match format
        /// </summary>
        private static int CalculateTotalSlots(MatchFormat format)
        {
            return format switch
            {
                MatchFormat.FiveVsFive => 10,      // 5 vs 5 = 10 players
                MatchFormat.SevenVsSeven => 14,    // 7 vs 7 = 14 players
                MatchFormat.ElevenVsEleven => 22,  // 11 vs 11 = 22 players
                _ => throw new ArgumentException($"Unknown match format: {format}")
            };
        }
    }
}
