using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.Announcements.Commands.DeleteAnnouncement;

public class DeleteAnnouncementCommand : ICommand<DeleteAnnouncementResponse>
{
    public Guid AnnouncementId { get; set; }
}
