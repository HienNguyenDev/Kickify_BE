using Kickify.Domain.Enums;

namespace Kickify.Api.Requests;

public class CreateAnnouncementRequest
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public AnnouncementType AnnouncementType { get; set; }
    public Priority Priority { get; set; } = Priority.Medium;
    public DateTime ShowFrom { get; set; }
    public DateTime? ShowTo { get; set; }
}
