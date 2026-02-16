using Kickify.Domain.Common;

namespace Kickify.Domain.Entities;

public class NotificationPreference : BaseEntity
{
    public Guid PreferenceId { get; set; }
    public Guid UserId { get; set; }
    public bool MatchRoom { get; set; } = true;
    public bool Friendship { get; set; } = true;
    public bool Post { get; set; } = true;

    // Navigation properties
    public User User { get; set; } = null!;
}
