namespace Kickify.Application.Features.MatchRooms.Commands.CreateMatchRoom
{
    public record CreateMatchRoomResponse(
        Guid RoomId,
        Guid HostId,
        Guid? FieldId,
        string? RoomName,
        DateTime MatchDate,
        TimeSpan StartTime,
        TimeSpan EndTime,
        int DurationMinutes,
        string MatchFormat,
        int TotalSlots,
        int FilledSlots,
        decimal DepositPerPerson,
        decimal TotalDepositCollected,
        string Visibility,
        bool IsPrivate,
        string Status,
        DateTime CreatedAt
    );
}
