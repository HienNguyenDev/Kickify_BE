using Kickify.Domain.Enums;

namespace Kickify.Domain.Entities;

public class ContentReport
{
    public Guid ReportId { get; set; }
    public Guid ReporterId { get; set; }
    public ContentType ContentType { get; set; }
    public Guid ContentId { get; set; } // PostId or CommentId
    public Guid ContentOwnerId { get; set; }
    public ContentReportReason Reason { get; set; }
    public string? Description { get; set; }
    public ReportStatus Status { get; set; } = ReportStatus.Pending;
    public string? AdminNotes { get; set; }
    public Guid? ResolvedBy { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public User Reporter { get; set; } = null!;
    public User ContentOwner { get; set; } = null!;
    public User? Resolver { get; set; }
}
