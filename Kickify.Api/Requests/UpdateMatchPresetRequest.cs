namespace Kickify.Api.Requests
{
    public class UpdateMatchPresetRequest
    {
        public string? PresetName { get; set; }
        public Guid? FieldId { get; set; }
        public string? CustomLocation { get; set; }
        public string? MatchFormat { get; set; }
        public int? DurationMinutes { get; set; }
        public string? Description { get; set; }
    }
}
