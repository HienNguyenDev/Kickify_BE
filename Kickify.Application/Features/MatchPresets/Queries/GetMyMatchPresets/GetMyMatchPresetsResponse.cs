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
        string PresetRoomName,
        string MatchFormat,
        string Visibility,
        string? RoomPassword,
        int DurationMinutes,
        string? Description,
        DateTime CreatedAt
    );
}
