using Kickify.Domain.Enums;

namespace Kickify.Api.Requests;

public class UpdateAnnouncementRequest
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public AnnouncementType AnnouncementType { get; set; }
    public Priority Priority { get; set; }
    public DateTime ShowFrom { get; set; }
    public DateTime? ShowTo { get; set; }
    public bool IsActive { get; set; }
}
