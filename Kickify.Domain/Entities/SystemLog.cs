namespace Kickify.Domain.Entities;

public class SystemLog
{
    public Guid LogId { get; set; }
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
    public SystemLogAction Action { get; set; }
    public string? EntityType { get; set; } // User, Match, Booking, etc.
    public Guid? EntityId { get; set; }
    public string? UserAgent { get; set; } // "mobile", "web", "unknown"
    public SystemLogResponseStatus ResponseStatus { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public User? User { get; set; }
}
