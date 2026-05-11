using System;
using System.Collections.Generic;

namespace Kickify.Application.Features.MatchRooms.Queries.GetPlayerMatchHistory
{
    public record GetPlayerMatchHistoryResponse(
        List<PlayerMatchRoomItemDto> Rooms,
        int Total,
        int Page,
        int PageSize,
        int TotalPages
    );

    public record PlayerMatchRoomItemDto(
        Guid RoomId,
        Guid HostId,
        PlayerRoomHostDto Host,
        Guid? FieldId,
        PlayerRoomFieldDto? Field,
        string? RoomName,
        DateTime MatchDate,
        TimeSpan StartTime,
        TimeSpan EndTime,
        int DurationMinutes,
        string MatchFormat,
        string? Description,
        int? TeamAScore,
        int? TeamBScore,
        string Status,
        DateTime CreatedAt,
        int TotalSlots,
        int FilledSlots,
        List<PlayerRoomVenuePhotoDto> VenuePhotos,
        decimal? CheckInLatitude,
        decimal? CheckInLongitude,
        string? CheckInMethod,
        //string? CheckInPhotoUrl,
        double? DistanceFromVenueMeters
    );

    public record PlayerRoomVenuePhotoDto(
        Guid PhotoId,
        string PhotoUrl,
        int DisplayOrder
    );

    public record PlayerRoomHostDto(
        Guid UserId,
        string FullName,
        string? AvatarUrl
    );

    public record PlayerRoomFieldDto(
        Guid FieldId,
        string FieldName,
        PlayerRoomVenueDto Venue
    );

    public record PlayerRoomVenueDto(
        Guid VenueId,
        string VenueName
    );
}
