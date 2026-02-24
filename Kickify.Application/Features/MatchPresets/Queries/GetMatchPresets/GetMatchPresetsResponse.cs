namespace Kickify.Application.Features.MatchPresets.Queries.GetMatchPresets
{
    public record GetMatchPresetsResponse(
        List<MatchPresetItemDto> Items,
        int Page,
        int PageSize,
        int TotalCount,
        int TotalPages
    );

    public record MatchPresetItemDto(
        Guid PresetId,
        Guid UserId,
        string UserName,
        string PresetName,
        Guid? FieldId,
        string? FieldName,
        string? VenueName,
        string? CustomLocation,
        string MatchFormat,
        int DurationMinutes,
        string? Description,
        DateTime CreatedAt
    );
}
