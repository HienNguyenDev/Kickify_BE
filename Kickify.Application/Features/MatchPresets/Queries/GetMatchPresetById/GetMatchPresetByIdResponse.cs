namespace Kickify.Application.Features.MatchPresets.Queries.GetMatchPresetById
{
    public record GetMatchPresetByIdResponse(
        Guid PresetId,
        Guid UserId,
        string UserName,
        Guid? FieldId,
        string? FieldName,
        string? VenueName,
        string? VenueAddress,
        string RoomName,
        string MatchFormat,
        string Visibility,
        string? Password,
        TimeSpan StartTime,
        string? Rules,
        int DurationMinutes,
        string? Description,
        DateTime CreatedAt
    );
}
