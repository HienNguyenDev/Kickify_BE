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
    public DateTime? UpdatedAt { get; set; } // Track when participant updates their team/position
    public bool IsCaptain { get; set; } = false; // Team captain flag
    public bool DepositPaid { get; set; } = false;
    public decimal? DepositAmount { get; set; }
    public bool CheckedIn { get; set; } = false;
    public DateTime? CheckInTime { get; set; }
    public decimal? CheckInLatitude { get; set; }
    public decimal? CheckInLongitude { get; set; }
    public string? CheckInMethod { get; set; } // "GPS" or "Photo"
    public string? CheckInPhotoUrl { get; set; }
    public double? DistanceFromVenueMeters { get; set; }
    public string? RemovalReason { get; set; }

    // Navigation properties
    public MatchRoom MatchRoom { get; set; } = null!;
    public User User { get; set; } = null!;
}
