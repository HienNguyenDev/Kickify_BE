namespace Kickify.Api.Requests
{
    public class CreateMatchPresetRequest
    {
        public string PresetRoomName { get; set; } = string.Empty;
        public string MatchFormat { get; set; } = string.Empty;
        public string? Visibility { get; set; }
        public int DurationMinutes { get; set; }
        public string? RoomPassword { get; set; }
        public string? Description { get; set; }
    }
}
