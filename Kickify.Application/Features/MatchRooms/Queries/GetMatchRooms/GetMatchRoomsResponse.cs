namespace Kickify.Application.Features.MatchRooms.Queries.GetMatchRooms
{
    public record GetMatchRoomsResponse(
        List<MatchRoomItemDto> Rooms,
        int Total,
        int Page,
        int PageSize
    );

    public record MatchRoomItemDto(
        Guid RoomId,
        Guid HostId,
        string HostName,
        string? HostAvatar,
        Guid? FieldId,
        string? FieldName,
        string? VenueName,
        string? VenueAddress,
        DateTime MatchDate,
        TimeSpan StartTime,
        TimeSpan EndTime,
        int DurationMinutes,
        string MatchFormat,
        int TotalSlots,
        int FilledSlots,
        decimal? DepositPerPerson,
        string Status,
        DateTime CreatedAt
    );
}
