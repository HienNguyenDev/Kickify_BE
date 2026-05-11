namespace Kickify.Api.Requests
{
    public class CreateMatchPresetRequest
    {
        public Guid FieldId { get; set; }
        public string RoomName { get; set; } = string.Empty;
        public string MatchFormat { get; set; } = string.Empty;
        public string? Visibility { get; set; }
        public TimeSpan StartTime { get; set; }
        public int DurationMinutes { get; set; }
        public string? Rules { get; set; }
        public string? Password { get; set; }
        public string? Description { get; set; }
    }
}
