namespace Kickify.Application.Features.MatchPresets.Queries.GetMatchPresetById
{
    public record GetMatchPresetByIdResponse(
        Guid PresetId,
        Guid UserId,
        string UserName,
        string PresetName,
        Guid? FieldId,
        PresetFieldDto? Field,
        string? CustomLocation,
        string MatchFormat,
        int DurationMinutes,
        string? Description,
        DateTime CreatedAt
    );

    public record PresetFieldDto(
        Guid FieldId,
        string FieldName,
        string FieldType,
        PresetVenueDto Venue
    );

    public record PresetVenueDto(
        Guid VenueId,
        string VenueName,
        string Address
    );
}
