namespace Kickify.Api.Requests
{
    public class CreateMatchPresetRequest
    {
        public string PresetName { get; set; } = string.Empty;
        public Guid? FieldId { get; set; }
        public string? CustomLocation { get; set; }
        public string MatchFormat { get; set; } = string.Empty;
        public int DurationMinutes { get; set; }
        public string? Description { get; set; }
    }
}
