namespace Kickify.Domain.Entities;

public class VenueEvidence
{
    public Guid EvidenceId { get; set; }
    public Guid VenueId { get; set; }
    public string FileUrl { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public Venue Venue { get; set; } = null!;
}
