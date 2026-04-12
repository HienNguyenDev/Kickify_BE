using System;
using System.Collections.Generic;

namespace Kickify.Application.Features.MatchRooms.Queries.GetRecommendedMatchRooms
{
    public record GetRecommendedMatchRoomsResponse(
        List<RecommendedMatchRoomItemDto> Rooms,
        int TotalCount,
        int Page,
        int PageSize,
        int TotalPages
    );

    public record RecommendedMatchRoomItemDto(
        Guid RoomId,
        string? RoomName,
        Guid HostId,
        string HostName,
        string? HostAvatar,
        Guid? FieldId,
        string? FieldName,
        string? VenueName,
        string? VenueAddress,
        List<RecommendedVenuePhotoDto> VenuePhotos,
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

    public record RecommendedVenuePhotoDto(
        Guid PhotoId,
        string PhotoUrl,
        int DisplayOrder
    );
}