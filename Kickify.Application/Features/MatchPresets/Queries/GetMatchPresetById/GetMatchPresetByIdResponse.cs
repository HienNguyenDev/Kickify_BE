namespace Kickify.Application.Features.MatchPresets.Queries.GetMatchPresetById
{
    public record GetMatchPresetByIdResponse(
        Guid PresetId,
        Guid UserId,
        string UserName,
        string PresetRoomName,
        string MatchFormat,
        string Visibility,
        string? RoomPassword,
        int DurationMinutes,
        string? Description,
        DateTime CreatedAt
    );
}
