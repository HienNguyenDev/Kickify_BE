namespace Kickify.Application.Features.MatchRooms.Queries.GetMyMatchRooms
{
    public record GetMyMatchRoomsResponse(
        List<MyMatchRoomItemDto> Rooms,
        int Total,
        int Page,
        int PageSize,
        int TotalPages
    );

    public record MyMatchRoomItemDto(
        Guid RoomId,
        Guid HostId,
        MyRoomHostDto Host,
        Guid? FieldId,
        MyRoomFieldDto? Field,
        string? RoomName,
        DateTime MatchDate,
        TimeSpan StartTime,
        TimeSpan EndTime,
        int DurationMinutes,
        string MatchFormat,
        string? Description,
        string? Rules,
        int TotalSlots,
        int FilledSlots,
        decimal? DepositPerPerson,
        decimal TotalDepositCollected,
        string Status,
        DateTime CreatedAt
    );

    public record MyRoomHostDto(
        Guid UserId,
        string FullName,
        string? AvatarUrl
    );

    public record MyRoomFieldDto(
        Guid FieldId,
        string FieldName,
        string FieldType,
        decimal HourlyRate,
        MyRoomVenueDto Venue
    );

    public record MyRoomVenueDto(
        Guid VenueId,
        string VenueName,
        string Address,
        string? ContactPhone
    );
}
