namespace Kickify.Domain.Entities;

public class FormationAssignment
{
    public Guid AssignmentId { get; set; }
    public Guid FormationId { get; set; }
    public Guid PlayerId { get; set; }
    public string SlotId { get; set; } = string.Empty; // e.g., "GK-0", "DF-1", "MF-0", "FW-0"

    // Navigation properties
    public MatchFormation Formation { get; set; } = null!;
    public User Player { get; set; } = null!;
}
