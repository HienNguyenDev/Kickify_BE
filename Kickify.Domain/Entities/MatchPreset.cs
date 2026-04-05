using Kickify.Domain.Enums;

namespace Kickify.Domain.Entities;

public class MatchPreset
{
    public Guid PresetId { get; set; }
    public Guid UserId { get; set; }
    public Guid? FieldId { get; set; }
    public string RoomName { get; set; } = string.Empty;
    public MatchFormat MatchFormat { get; set; }
    public Visibility Visibility { get; set; } = Visibility.Public;
    public string? Password { get; set; }
    public TimeSpan StartTime { get; set; }
    public string? Rules { get; set; }
    public int DurationMinutes { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public Field? Field { get; set; }
}
