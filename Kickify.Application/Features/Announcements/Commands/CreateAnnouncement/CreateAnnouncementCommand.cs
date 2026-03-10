using Kickify.Application.Abstractions.Messaging;
using Kickify.Domain.Enums;

namespace Kickify.Application.Features.Announcements.Commands.CreateAnnouncement;

public class CreateAnnouncementCommand : ICommand<CreateAnnouncementResponse>
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public AnnouncementType AnnouncementType { get; set; }
    public Priority Priority { get; set; } = Priority.Medium;
    public DateTime ShowFrom { get; set; }
    public DateTime? ShowTo { get; set; }
}
