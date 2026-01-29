namespace Kickify.Application.Features.MatchPresets.Queries.GetMyMatchPresets
{
    public record GetMyMatchPresetsResponse(
        List<MyMatchPresetItemDto> Items,
        int Page,
        int PageSize,
        int TotalCount,
        int TotalPages
    );

    public record MyMatchPresetItemDto(
        Guid PresetId,
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
