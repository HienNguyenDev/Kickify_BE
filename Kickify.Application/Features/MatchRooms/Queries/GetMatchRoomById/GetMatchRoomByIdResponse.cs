namespace Kickify.Application.Features.MatchRooms.Queries.GetMatchRoomById
{
    /// <summary>Room detail including team average Elo (0 if no one on that team) and skill-imbalance when both teams have players and Elo differs by &gt; 200.</summary>
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
        string Visibility,
        bool IsPrivate,
        string Status,
        RoomParticipantsDto Participants,
        RoomFormationsDto? Formations,
        decimal TeamAAverageElo,
        decimal TeamBAverageElo,
        bool IsSkillImbalanced,
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
        DateTime JoinDate,
        bool HasLeftFeedback,
        decimal? CheckInLatitude,
        decimal? CheckInLongitude,
        string? CheckInMethod,
        //string? CheckInPhotoUrl,
        double? DistanceFromVenueMeters
    );

    public record RoomFormationsDto(
        RoomTeamFormationDto? TeamA,
        RoomTeamFormationDto? TeamB
    );

    public record RoomTeamFormationDto(
        string? TeamName,
        string? FormationName,
        List<FormationAssignmentDto> Assignments
    );

    public record FormationAssignmentDto(
        Guid PlayerId,
        string PlayerName,
        string SlotId,
        string Position
    );
}
