namespace Kickify.Application.Features.MatchRooms.Queries.GetVenueMatchRooms;

public record GetVenueMatchRoomsResponse(
    List<VenueMatchRoomItemDto> Rooms,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);

public record VenueMatchRoomItemDto(
    Guid RoomId,
    string? RoomName,
    Guid? FieldId,
    string? FieldName,
    Guid? VenueId,
    string? VenueName,
    string HostName,
    string? HostAvatar,
    DateTime MatchDate,
    TimeSpan StartTime,
    TimeSpan EndTime,
    int DurationMinutes,
    string MatchFormat,
    int TotalSlots,
    int FilledSlots,
    decimal? DepositPerPerson,
    decimal TotalDepositCollected,
    string Status,
    string? BookingStatus,
    decimal? BookingTotalAmount,
    string? FinalResult,
    DateTime CreatedAt
);
