namespace Kickify.Api.Requests
{
    public record CreateMatchRoomRequest(
        Guid FieldId,
        DateTime MatchDate,
        TimeSpan StartTime,
        int DurationMinutes,
        string MatchFormat,
        string? RoomName,
        string? Description,
        string? Rules,
        decimal? DepositPerPerson
    );
}
