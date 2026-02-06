namespace Kickify.Application.Features.MatchRooms.Queries.GetMatchRoomById
{
    public record GetMatchRoomByIdResponse(
        Guid RoomId,
        Guid HostId,
        RoomHostDto Host,
        Guid? FieldId,
        RoomFieldDto? Field,
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
        RoomParticipantsDto Participants,
        DateTime CreatedAt
    );

    public record RoomParticipantsDto(
        List<RoomParticipantDto> TeamA,
        List<RoomParticipantDto> TeamB,
        List<RoomParticipantDto> Unassigned
    );

    public record RoomHostDto(
        Guid UserId,
        string FullName,
        string? AvatarUrl
    );

    public record RoomFieldDto(
        Guid FieldId,
        string FieldName,
        string FieldType,
        decimal HourlyRate,
        RoomVenueDto Venue
    );

    public record RoomVenueDto(
        Guid VenueId,
        string VenueName,
        string Address,
        string? ContactPhone
    );

    public record RoomParticipantDto(
        Guid ParticipantId,
        Guid UserId,
        string FullName,
        string? AvatarUrl,
        string TeamAssignment,
        string? Position,
        bool DepositPaid,
        bool CheckedIn,
        DateTime? CheckInTime,
        bool IsCaptain,
        DateTime JoinDate
    );
}
