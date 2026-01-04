using Kickify.Domain.Common;
using Kickify.Domain.Enums;

namespace Kickify.Domain.Entities;

public class Announcement : BaseEntity
{
    public Guid AnnouncementId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public AnnouncementType AnnouncementType { get; set; }
    public Priority Priority { get; set; } = Priority.Medium;
    public DateTime ShowFrom { get; set; }
    public DateTime? ShowTo { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid CreatedBy { get; set; }
    // Navigation properties
    public User Creator { get; set; } = null!;
}
