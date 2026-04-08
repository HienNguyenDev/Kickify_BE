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
