using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Abstractions.Jobs;
using Kickify.Application.Common.Pricing;
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
    private readonly IHolidayRepository _holidayRepository;
    private readonly IUserRepository _userRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly IRoomParticipantRepository _roomParticipantRepository;
    private readonly IRoomAutoCloseService _roomAutoCloseService;
    private readonly IMatchLifecycleService _matchLifecycleService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;

    public CreateMatchRoomCommandHandler(
        IMatchRoomRepository matchRoomRepository,
        IFieldRepository fieldRepository,
        IHolidayRepository holidayRepository,
        IUserRepository userRepository,
        IBookingRepository bookingRepository,
        IRoomParticipantRepository roomParticipantRepository,
        IRoomAutoCloseService roomAutoCloseService,
        IMatchLifecycleService matchLifecycleService,
        IUnitOfWork unitOfWork,
        IUserContext userContext)
    {
        _matchRoomRepository = matchRoomRepository;
        _fieldRepository = fieldRepository;
        _holidayRepository = holidayRepository;
        _userRepository = userRepository;
        _bookingRepository = bookingRepository;
        _roomParticipantRepository = roomParticipantRepository;
        _roomAutoCloseService = roomAutoCloseService;
        _matchLifecycleService = matchLifecycleService;
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
            return Result.Failure<CreateMatchRoomResponse>(MatchRoomErrors.VenueClosed(request.MatchDate));
        }

        var openTime = operatingHour.OpenTime ?? TimeSpan.Zero;
        var closeTime = operatingHour.CloseTime ?? TimeSpan.Zero;

        if (request.StartTime < openTime || endTime > closeTime)
        {
            return Result.Failure<CreateMatchRoomResponse>(
                MatchRoomErrors.OutsideOperatingHours(request.StartTime, endTime, openTime, closeTime));
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
                MatchRoomErrors.SlotAlreadyBooked(request.StartTime, endTime));
        }

        if (!Enum.TryParse<MatchFormat>(request.MatchFormat, true, out var matchFormat))
        {
            return Result.Failure<CreateMatchRoomResponse>(MatchRoomErrors.InvalidFormat(request.MatchFormat));
        }

        var visibility = Visibility.Public;
        if (!string.IsNullOrEmpty(request.Visibility) && !Enum.TryParse<Visibility>(request.Visibility, true, out visibility))
        {
            return Result.Failure<CreateMatchRoomResponse>(MatchRoomErrors.InvalidVisibility(request.Visibility));
        }

        int totalSlots = CalculateTotalSlots(matchFormat);
        var holiday = await _holidayRepository.GetByDateAsync(request.MatchDate, cancellationToken);
        var priceResult = MatchPriceCalculator.CalculateMatchPrice(
            field,
            request.MatchDate,
            request.StartTime,
            request.DurationMinutes,
            holiday);

        var totalAmount = priceResult.TotalPrice;
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

        var booking = new Booking
        {
            BookingId = Guid.NewGuid(),
            FieldId = request.FieldId,
            RoomId = room.RoomId,
            BookingDate = request.MatchDate,
            StartTime = request.StartTime,
            EndTime = endTime,
            TotalAmount = totalAmount,
            Status = BookingStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
        await _bookingRepository.AddAsync(booking);

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

        //var now = DateTime.UtcNow;
        //// Make sure it handles correctly based on standard DateTime comparisons
        //var matchStartDateTime = request.MatchDate.Date.Add(request.StartTime);
        //var timeToMatchStartMinus2h = matchStartDateTime.AddHours(-2) - now;
        //var timeTo24h = TimeSpan.FromHours(24);

        //var calculatedDelay = timeToMatchStartMinus2h < timeTo24h ? timeToMatchStartMinus2h : timeTo24h;
        //if (calculatedDelay <= TimeSpan.Zero) 
        //{
        //    calculatedDelay = TimeSpan.FromMinutes(15);
        //}

        //_roomAutoCloseService.ScheduleAutoClose(room.RoomId, calculatedDelay);

        //var matchStartTime = room.MatchDate.Add(room.StartTime);
        //_matchLifecycleService.SchedulePreMatchReminders(room.RoomId, matchStartTime);
        var utcNow = DateTime.UtcNow;

        // 1. Xác định múi giờ Việt Nam (giống hàm CheckAvailability)
        TimeZoneInfo vnTimeZone;
        try
        {
            vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"); // Windows
        }
        catch (TimeZoneNotFoundException)
        {
            vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh"); // Linux/Docker
        }

        // 2. Lấy giờ đá do User nhập (Giờ VN) và ép kiểu nó thành DateTimeKind.Unspecified
        // để báo cho C# biết con số này chưa gắn với múi giờ nào.
        var matchStartVietnam = DateTime.SpecifyKind(request.MatchDate.Date.Add(request.StartTime), DateTimeKind.Unspecified);

        // 3. Chuyển đổi giờ đá từ VN sang chuẩn UTC
        var matchStartUtc = TimeZoneInfo.ConvertTimeToUtc(matchStartVietnam, vnTimeZone);

        // 4. BÂY GIỜ MỚI LÀM PHÉP TRỪ (Cả 2 đều đang ở hệ UTC+0)
        var timeToMatchStartMinus2h = matchStartUtc.AddHours(-2) - utcNow;
        var timeTo24h = TimeSpan.FromHours(24);

        var calculatedDelay = timeToMatchStartMinus2h < timeTo24h ? timeToMatchStartMinus2h : timeTo24h;
        if (calculatedDelay <= TimeSpan.Zero)
        {
            // Nếu tạo phòng quá sát giờ đá (< 2 tiếng), cho họ 15 phút để gom người
            calculatedDelay = TimeSpan.FromMinutes(15);
        }

        // Lên lịch Hangfire với số giây delay chuẩn xác
        _roomAutoCloseService.ScheduleAutoClose(room.RoomId, calculatedDelay);

        // Tương tự, nhắc nhở cũng phải lên lịch theo hệ UTC (nếu hàm này dùng Hangfire background job)
        _matchLifecycleService.SchedulePreMatchReminders(room.RoomId, matchStartUtc);
       

        // Quy định: Phải tạo phòng trước giờ bóng lăn ÍT NHẤT 30 phút
        var minAdvanceTime = TimeSpan.FromMinutes(30);

        if (matchStartUtc <= utcNow)
        {
            return Result.Failure<CreateMatchRoomResponse>(MatchRoomErrors.InvalidTime);
        }

        if (matchStartUtc - utcNow < minAdvanceTime)
        {
            return Result.Failure<CreateMatchRoomResponse>(MatchRoomErrors.TooCloseToStartTime);
        }

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
