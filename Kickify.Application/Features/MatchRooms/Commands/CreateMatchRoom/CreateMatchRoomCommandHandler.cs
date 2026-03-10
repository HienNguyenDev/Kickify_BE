using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;
using Kickify.Domain.Event;

namespace Kickify.Application.Features.MatchRooms.Commands.CreateMatchRoom;

public class CreateMatchRoomCommandHandler : ICommandHandler<CreateMatchRoomCommand, CreateMatchRoomResponse>
{
    private readonly IMatchRoomRepository _matchRoomRepository;
    private readonly IFieldRepository _fieldRepository;
    private readonly IUserRepository _userRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly IRoomParticipantRepository _roomParticipantRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;

    public CreateMatchRoomCommandHandler(
        IMatchRoomRepository matchRoomRepository,
        IFieldRepository fieldRepository,
        IUserRepository userRepository,
        IBookingRepository bookingRepository,
        IRoomParticipantRepository roomParticipantRepository,
        IUnitOfWork unitOfWork,
        IUserContext userContext)
    {
        _matchRoomRepository = matchRoomRepository;
        _fieldRepository = fieldRepository;
        _userRepository = userRepository;
        _bookingRepository = bookingRepository;
        _roomParticipantRepository = roomParticipantRepository;
        _unitOfWork = unitOfWork;
        _userContext = userContext;
    }

    public async Task<Result<CreateMatchRoomResponse>> Handle(CreateMatchRoomCommand request, CancellationToken cancellationToken)
    {
        var userId = _userContext.UserId;
        
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return Result.Failure<CreateMatchRoomResponse>(UserErrors.NotFound(userId));
        }

        var field = await _fieldRepository.GetFieldWithVenueAsync(request.FieldId, cancellationToken);
        if (field == null)
        {
            return Result.Failure<CreateMatchRoomResponse>(FieldErrors.NotFound(request.FieldId));
        }

        // Check if venue is archived
        if (field.Venue.Status == VenueStatus.Archived)
        {
            return Result.Failure<CreateMatchRoomResponse>(MatchRoomErrors.VenueArchived);
        }

        var endTime = request.StartTime.Add(TimeSpan.FromMinutes(request.DurationMinutes));

        var dayOfWeek = (DayOfWeekEnum)request.MatchDate.DayOfWeek;
        var operatingHour = field.Venue.VenueOperatingHours
            .FirstOrDefault(oh => oh.DayOfWeek == dayOfWeek);

        if (operatingHour == null || operatingHour.IsClosed)
        {
            return Result.Failure<CreateMatchRoomResponse>(
                new Error("MatchRoom.VenueClosed", $"Venue is closed on {request.MatchDate:dddd}", ErrorType.Validation));
        }

        var openTime = operatingHour.OpenTime ?? TimeSpan.Zero;
        var closeTime = operatingHour.CloseTime ?? TimeSpan.Zero;

        if (request.StartTime < openTime || endTime > closeTime)
        {
            return Result.Failure<CreateMatchRoomResponse>(
                new Error("MatchRoom.OutsideOperatingHours", 
                    $"Requested time ({request.StartTime:hh\\:mm} - {endTime:hh\\:mm}) is outside operating hours ({openTime:hh\\:mm} - {closeTime:hh\\:mm})", 
                    ErrorType.Validation));
        }

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

        if (!Enum.TryParse<MatchFormat>(request.MatchFormat, true, out var matchFormat))
        {
            return Result.Failure<CreateMatchRoomResponse>(MatchRoomErrors.InvalidFormat(request.MatchFormat));
        }

        var visibility = Visibility.Public;
        if (!string.IsNullOrEmpty(request.Visibility) && !Enum.TryParse<Visibility>(request.Visibility, true, out visibility))
        {
            return Result.Failure<CreateMatchRoomResponse>(
                new Error("MatchRoom.InvalidVisibility", "Visibility must be Public or Private", ErrorType.Validation));
        }

        int totalSlots = CalculateTotalSlots(matchFormat);

        var durationHours = (decimal)request.DurationMinutes / 60;
        var totalAmount = field.HourlyRate * durationHours;
        var depositPerPerson = Math.Round(totalAmount / totalSlots, 0);

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
            FilledSlots = 1,
            Description = request.Description,
            Rules = request.Rules,
            DepositPerPerson = depositPerPerson,
            TotalDepositCollected = 0,
            Visibility = visibility,
            RoomPassword = visibility == Visibility.Private ? request.Password : null,
            Status = RoomStatus.Open,
            CreatedAt = DateTime.UtcNow
        };

        room.Raise(new MatchRoomCreatedDomainEvent(room.RoomId));

        await _matchRoomRepository.AddAsync(room);

        var hostParticipant = new RoomParticipant
        {
            ParticipantId = Guid.NewGuid(),
            RoomId = room.RoomId,
            UserId = userId,
            TeamAssignment = TeamAssignment.Unassigned,
            JoinDate = DateTime.UtcNow,
            DepositPaid = false,
            DepositAmount = depositPerPerson
        };

        await _roomParticipantRepository.AddAsync(hostParticipant);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

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
            room.DepositPerPerson ?? 0,
            room.TotalDepositCollected,
            room.Visibility.ToString(),
            room.Visibility == Visibility.Private,
            room.Status.ToString(),
            room.CreatedAt
        ));
    }

    private static int CalculateTotalSlots(MatchFormat format)
    {
        return format switch
        {
            MatchFormat.FiveVsFive => 10,
            MatchFormat.SevenVsSeven => 14,
            MatchFormat.ElevenVsEleven => 22,
            _ => throw new ArgumentException($"Unknown match format: {format}")
        };
    }
}
