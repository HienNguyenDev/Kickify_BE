namespace Kickify.Application.Features.MatchRooms.Queries.GetMatchRooms
{
    public record GetMatchRoomsResponse(
        List<MatchRoomItemDto> Rooms,
        int TotalCount,
        int Page,
        int PageSize,
        int TotalPages
    );

    public record MatchRoomItemDto(
        Guid RoomId,
        string? RoomName,
        Guid HostId,
        string HostName,
        string? HostAvatar,
        Guid? FieldId,
        string? FieldName,
        string? VenueName,
        string? VenueAddress,
        List<VenuePhotoDto> VenuePhotos,
        DateTime MatchDate,
        TimeSpan StartTime,
        TimeSpan EndTime,
        int DurationMinutes,
        string MatchFormat,
        int TotalSlots,
        int FilledSlots,
        decimal? DepositPerPerson,
        string Visibility,
        bool IsPrivate,
        string Status,
        DateTime CreatedAt
    );

    public record VenuePhotoDto(
        Guid PhotoId,
        string PhotoUrl,
        int DisplayOrder
    );
}
