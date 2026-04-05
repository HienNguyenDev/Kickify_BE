namespace Kickify.Api.Requests
{
    public class UpdateMatchPresetRequest
    {
        public Guid? FieldId { get; set; }
        public string? RoomName { get; set; }
        public string? MatchFormat { get; set; }
        public string? Visibility { get; set; }
        public TimeSpan? StartTime { get; set; }
        public int? DurationMinutes { get; set; }
        public string? Rules { get; set; }
        public string? Password { get; set; }
        public string? Description { get; set; }
    }
}
