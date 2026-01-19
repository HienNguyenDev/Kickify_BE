namespace Kickify.Application.Features.MatchRooms.Commands.CreateMatchRoom
{
    public record CreateMatchRoomResponse(
        Guid RoomId,
        Guid HostId,
        Guid? FieldId,
        DateTime MatchDate,
        TimeSpan StartTime,
        TimeSpan EndTime,
        int DurationMinutes,
        string MatchFormat,
        int TotalSlots,
        int FilledSlots,
        string Status,
        DateTime CreatedAt
    );
}
