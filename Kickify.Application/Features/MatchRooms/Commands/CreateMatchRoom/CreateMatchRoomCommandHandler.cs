using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace Kickify.Application.Features.MatchRooms.Commands.CreateMatchRoom
{
    public class CreateMatchRoomCommandHandler : ICommandHandler<CreateMatchRoomCommand, CreateMatchRoomResponse>
    {
        private readonly IMatchRoomRepository _matchRoomRepository;
        private readonly IFieldRepository _fieldRepository;
        private readonly IUserRepository _userRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IRoomParticipantRepository _roomParticipantRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserContext _userContext;
        private readonly ILogger<CreateMatchRoomCommandHandler> _logger;

        public CreateMatchRoomCommandHandler(
            IMatchRoomRepository matchRoomRepository,
            IFieldRepository fieldRepository,
            IUserRepository userRepository,
            IBookingRepository bookingRepository,
            IRoomParticipantRepository roomParticipantRepository,
            IUnitOfWork unitOfWork,
            IUserContext userContext,
            ILogger<CreateMatchRoomCommandHandler> logger)
        {
            _matchRoomRepository = matchRoomRepository;
            _fieldRepository = fieldRepository;
            _userRepository = userRepository;
            _bookingRepository = bookingRepository;
            _roomParticipantRepository = roomParticipantRepository;
            _unitOfWork = unitOfWork;
            _userContext = userContext;
            _logger = logger;
        }

        public async Task<Result<CreateMatchRoomResponse>> Handle(CreateMatchRoomCommand request, CancellationToken cancellationToken)
        {
            var userId = _userContext.UserId;
            
            // Verify user exists
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return Result.Failure<CreateMatchRoomResponse>(UserErrors.NotFound(userId));
            }

            // Verify field exists and get venue with operating hours
            var field = await _fieldRepository.GetFieldWithVenueAsync(request.FieldId, cancellationToken);
            if (field == null)
            {
                return Result.Failure<CreateMatchRoomResponse>(FieldErrors.NotFound(request.FieldId));
            }

            // Calculate end time for validation
            var endTime = request.StartTime.Add(TimeSpan.FromMinutes(request.DurationMinutes));

            // VALIDATION #1: Check if venue is open on this day
            var dayOfWeek = (DayOfWeekEnum)request.MatchDate.DayOfWeek;
            var operatingHour = field.Venue.VenueOperatingHours
                .FirstOrDefault(oh => oh.DayOfWeek == dayOfWeek);

            if (operatingHour == null || operatingHour.IsClosed)
            {
                return Result.Failure<CreateMatchRoomResponse>(
                    new Error("MatchRoom.VenueClosed", $"Venue is closed on {request.MatchDate:dddd}", ErrorType.Validation));
            }

            // VALIDATION #2: Check if time slot is within operating hours
            var openTime = operatingHour.OpenTime ?? TimeSpan.Zero;
            var closeTime = operatingHour.CloseTime ?? TimeSpan.Zero;

            if (request.StartTime < openTime || endTime > closeTime)
            {
                return Result.Failure<CreateMatchRoomResponse>(
                    new Error("MatchRoom.OutsideOperatingHours", 
                        $"Requested time ({request.StartTime:hh\\:mm} - {endTime:hh\\:mm}) is outside operating hours ({openTime:hh\\:mm} - {closeTime:hh\\:mm})", 
                        ErrorType.Validation));
            }

            // VALIDATION #3: Check if time slot is available (not already booked)
            bool isSlotAvailable = await _bookingRepository.IsTimeSlotAvailableAsync(
                request.FieldId,
                request.MatchDate,
                request.StartTime,
                endTime,
                cancellationToken);

            if (!isSlotAvailable)
            {
                return Result.Failure<CreateMatchRoomResponse>(
                    new Error("MatchRoom.SlotAlreadyBooked", 
                        $"Time slot {request.StartTime:hh\\:mm} - {endTime:hh\\:mm} is already booked by another room", 
                        ErrorType.Conflict));
            }

            // Parse MatchFormat enum
            if (!Enum.TryParse<MatchFormat>(request.MatchFormat, true, out var matchFormat))
            {
                return Result.Failure<CreateMatchRoomResponse>(MatchRoomErrors.InvalidFormat(request.MatchFormat));
            }

            // RULE #1: Auto-calculate TotalSlots based on MatchFormat
            int totalSlots = CalculateTotalSlots(matchFormat);

            try
            {
                // Create Match Room
                var room = new MatchRoom
                {
                    RoomId = Guid.NewGuid(),
                    HostId = userId,
                    FieldId = request.FieldId,
                    RoomName = request.RoomName,
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
                    UserId = userId,
                    TeamAssignment = TeamAssignment.Unassigned,
                    JoinDate = DateTime.UtcNow,
                    DepositPaid = false,
                    DepositAmount = request.DepositPerPerson
                };

                // Add host participant via repository
                await _roomParticipantRepository.AddAsync(hostParticipant);
                await _matchRoomRepository.AddAsync(room);

                // Save all changes in transaction
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Match room {RoomId} created by user {UserId} with {TotalSlots} slots",
                    room.RoomId, userId, totalSlots);

                return Result.Success(new CreateMatchRoomResponse(
                    room.RoomId,
                    room.HostId,
                    room.FieldId,
                    room.RoomName,
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
