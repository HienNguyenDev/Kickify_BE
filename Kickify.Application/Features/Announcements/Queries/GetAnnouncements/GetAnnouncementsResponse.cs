using Kickify.Domain.Enums;

namespace Kickify.Application.Features.Announcements.Queries.GetAnnouncements;

public class GetAnnouncementsResponse
{
    public List<AnnouncementDto> Announcements { get; set; } = [];
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

public class AnnouncementDto
{
    public Guid AnnouncementId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string AnnouncementType { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public DateTime ShowFrom { get; set; }
    public DateTime? ShowTo { get; set; }
    public bool IsActive { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
