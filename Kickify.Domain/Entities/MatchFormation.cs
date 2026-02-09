using Kickify.Domain.Enums;

namespace Kickify.Domain.Entities;

public class MatchFormation
{
    public Guid FormationId { get; set; }
    public Guid RoomId { get; set; }
    public TeamAssignment TeamAssignment { get; set; }
    public string FormationName { get; set; } = string.Empty; // e.g., "3-2-1"
    public string MatchFormat { get; set; } = string.Empty; // e.g., "7vs7"

    // Navigation properties
    public MatchRoom MatchRoom { get; set; } = null!;
    public ICollection<FormationAssignment> Assignments { get; set; } = new List<FormationAssignment>();
}
