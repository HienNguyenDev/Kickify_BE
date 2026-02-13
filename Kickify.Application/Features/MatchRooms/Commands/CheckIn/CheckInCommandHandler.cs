using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Jobs;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Abstractions.Services;
using Kickify.Domain.Common;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace Kickify.Application.Features.MatchRooms.Commands.CheckIn;

public class CheckInCommandHandler : ICommandHandler<CheckInCommand, CheckInResponse>
{
    private readonly IMatchRoomRepository _matchRoomRepository;
    private readonly IRoomParticipantRepository _roomParticipantRepository;
    private readonly IMatchLifecycleService _matchLifecycleService;
    private readonly IMatchRoomHubService _matchRoomHubService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;
    private readonly ILogger<CheckInCommandHandler> _logger;

    private static readonly TimeSpan CheckInWindowBeforeMatch = TimeSpan.FromMinutes(30);

    public CheckInCommandHandler(
        IMatchRoomRepository matchRoomRepository,
        IRoomParticipantRepository roomParticipantRepository,
        IMatchLifecycleService matchLifecycleService,
        IMatchRoomHubService matchRoomHubService,
        IUnitOfWork unitOfWork,
        IUserContext userContext,
        ILogger<CheckInCommandHandler> logger)
    {
        _matchRoomRepository = matchRoomRepository;
        _roomParticipantRepository = roomParticipantRepository;
        _matchLifecycleService = matchLifecycleService;
        _matchRoomHubService = matchRoomHubService;
        _unitOfWork = unitOfWork;
        _userContext = userContext;
        _logger = logger;
    }

    public async Task<Result<CheckInResponse>> Handle(CheckInCommand request, CancellationToken cancellationToken)
    {
        var userId = _userContext.UserId;

        // Get room
        var room = await _matchRoomRepository.GetByIdAsync(request.RoomId);
        if (room == null)
        {
            return Result.Failure<CheckInResponse>(MatchRoomErrors.NotFound(request.RoomId));
        }

        // Check room status - only allow check-in for Open or Locked rooms
        if (room.Status != RoomStatus.Open && room.Status != RoomStatus.Locked)
        {
            return Result.Failure<CheckInResponse>(MatchRoomErrors.CheckInNotAllowed);
        }

        // Check if within check-in window (30 minutes before match start)
        var matchStartDateTime = room.MatchDate.Add(room.StartTime);
        var checkInOpenTime = matchStartDateTime.Subtract(CheckInWindowBeforeMatch);
        var now = DateTime.UtcNow;

        if (now < checkInOpenTime)
        {
            return Result.Failure<CheckInResponse>(MatchRoomErrors.CheckInTooEarly);
        }

        // Get participant
        var participant = await _roomParticipantRepository.GetParticipantByRoomAndUserAsync(request.RoomId, userId, cancellationToken);
        if (participant == null)
        {
            return Result.Failure<CheckInResponse>(MatchRoomErrors.NotParticipant);
        }

        // Check if already checked in
        if (participant.CheckedIn)
        {
            return Result.Failure<CheckInResponse>(MatchRoomErrors.AlreadyCheckedIn);
        }

        // Perform check-in
        participant.CheckedIn = true;
        participant.CheckInTime = now;
        _roomParticipantRepository.Update(participant);

        // Get check-in count
        var checkedInCount = await _roomParticipantRepository.GetCheckedInCountAsync(request.RoomId, cancellationToken) + 1; // +1 for current check-in
        var totalParticipants = room.FilledSlots;
        var allCheckedIn = checkedInCount >= totalParticipants;

        // If all players checked in, lock the room and schedule match start
        if (allCheckedIn && room.Status == RoomStatus.Open)
        {
            room.Status = RoomStatus.Locked;
            _matchRoomRepository.Update(room);

            _logger.LogInformation("All players checked in for room {RoomId}. Room locked.", request.RoomId);

            // Schedule match start at the actual start time
            _matchLifecycleService.ScheduleMatchStart(request.RoomId, matchStartDateTime);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {UserId} checked in to room {RoomId}. CheckedIn: {CheckedIn}/{Total}",
            userId, request.RoomId, checkedInCount, totalParticipants);

        // Notify other participants
        await _matchRoomHubService.NotifyPlayerCheckedInAsync(
            request.RoomId,
            userId,
            checkedInCount,
            totalParticipants,
            allCheckedIn,
            cancellationToken);

        return Result.Success(new CheckInResponse(
            request.RoomId,
            userId,
            participant.CheckInTime!.Value,
            checkedInCount,
            totalParticipants,
            allCheckedIn,
            room.Status.ToString(),
            allCheckedIn ? "T?t c? ng??i ch?i ?ã check-in. Phòng ?ã khóa, ch? b?t ??u tr?n ??u." : "Check-in thành công."
        ));
    }
}
