using Kickify.Domain.Common;

namespace Kickify.Domain.Entities;

public class NotificationPreference : BaseEntity
{
    public Guid PreferenceId { get; set; }
    public Guid UserId { get; set; }
    public bool MatchInvites { get; set; } = true;
    public bool MatchResults { get; set; } = true;
    public bool ChatMessages { get; set; } = true;
    public bool RoomUpdates { get; set; } = true;
    public bool SystemAnnouncements { get; set; } = true;

    // Navigation properties
    public User User { get; set; } = null!;
}
