using Kickify.Domain.Enums;

namespace Kickify.Application.Features.Announcements.Commands.CreateAnnouncement;

public class CreateAnnouncementResponse
{
    public Guid AnnouncementId { get; set; }
    public string Title { get; set; } = string.Empty;
    public AnnouncementType AnnouncementType { get; set; }
    public DateTime ShowFrom { get; set; }
    public DateTime? ShowTo { get; set; }
}
