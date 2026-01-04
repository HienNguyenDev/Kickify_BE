using Kickify.Domain.Enums;

namespace Kickify.Domain.Entities;

public class PlayerReport
{
    public Guid ReportId { get; set; }
    public Guid ReporterId { get; set; }
    public Guid ReportedId { get; set; }
    public Guid? MatchId { get; set; }
    public ReportType ReportType { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? EvidenceUrls { get; set; } // JSON array of screenshot URLs
    public ReportStatus Status { get; set; } = ReportStatus.Pending;
    public string? AdminNotes { get; set; }
    public string? ActionTaken { get; set; }
    public Guid? ResolvedBy { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public User Reporter { get; set; } = null!;
    public User Reported { get; set; } = null!;
    public MatchRoom? Match { get; set; }
    public User? Resolver { get; set; }
}
