namespace Kickify.Application.Features.MatchRooms.Commands.CheckIn;

public record CheckInResponse(
    Guid RoomId,
    Guid UserId,
    DateTime CheckInTime,
    int CheckedInCount,
    int TotalParticipants,
    bool AllCheckedIn,
    string RoomStatus,
    string Message);
