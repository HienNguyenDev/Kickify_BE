using Kickify.Application.Abstractions.Messaging;
using Kickify.Domain.Enums;

namespace Kickify.Application.Features.Announcements.Queries.GetAnnouncements;

public class GetAnnouncementsQuery : IQuery<GetAnnouncementsResponse>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public AnnouncementType? AnnouncementType { get; set; }
    public bool? IsActive { get; set; }
}
