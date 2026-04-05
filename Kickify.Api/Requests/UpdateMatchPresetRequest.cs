namespace Kickify.Api.Requests
{
    public class UpdateMatchPresetRequest
    {
        public string? PresetRoomName { get; set; }
        public string? MatchFormat { get; set; }
        public string? Visibility { get; set; }
        public int? DurationMinutes { get; set; }
        public string? RoomPassword { get; set; }
        public string? Description { get; set; }
    }
}
