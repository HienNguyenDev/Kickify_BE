using Kickify.Domain.Enums;

namespace Kickify.Domain.Entities;

public class RoomParticipant
{
    public Guid ParticipantId { get; set; }
    public Guid RoomId { get; set; }
    public Guid UserId { get; set; }
    public TeamAssignment TeamAssignment { get; set; } = TeamAssignment.Unassigned;
    public string? Position { get; set; } // ST, CM, CB, etc.
    public DateTime JoinDate { get; set; }
    public bool DepositPaid { get; set; } = false;
    public decimal? DepositAmount { get; set; }
    public bool CheckedIn { get; set; } = false;
    public DateTime? CheckInTime { get; set; }
    public string? RemovalReason { get; set; }

    // Navigation properties
    public MatchRoom MatchRoom { get; set; } = null!;
    public User User { get; set; } = null!;
}
