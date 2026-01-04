using Kickify.Domain.Enums;

namespace Kickify.Domain.Entities;

public class MatchPreset
{
    public Guid PresetId { get; set; }
    public Guid UserId { get; set; }
    public string PresetName { get; set; } = string.Empty;
    public Guid? FieldId { get; set; }
    public string? CustomLocation { get; set; }
    public MatchFormat MatchFormat { get; set; }
    public int DurationMinutes { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public Field? Field { get; set; }
}
