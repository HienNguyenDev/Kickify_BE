using Kickify.Domain.Common;
using Kickify.Domain.Enums;

namespace Kickify.Domain.Entities;

public class MatchRoom : BaseEntity
{
    public Guid RoomId { get; set; }
    public Guid HostId { get; set; }
    public Guid? FieldId { get; set; }
    public string? RoomName { get; set; }
    public string? CustomLocation { get; set; }
    public MatchFormat MatchFormat { get; set; }
    public Enums.MatchType MatchType { get; set; }
    public Visibility Visibility { get; set; } = Visibility.Public;
    public DateTime MatchDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public int DurationMinutes { get; set; }
    public string? Description { get; set; }
    public string? Rules { get; set; }
    public int TotalSlots { get; set; }
    public int FilledSlots { get; set; } = 0;
    public decimal? DepositPerPerson { get; set; }
    public decimal TotalDepositCollected { get; set; } = 0;
    public RoomStatus Status { get; set; } = RoomStatus.Open;
    public int? TeamAScore { get; set; }
    public int? TeamBScore { get; set; }
    public int ResultConfirmedBy { get; set; } = 0; // count of confirmations

    // Navigation properties
    public User Host { get; set; } = null!;
    public Field? Field { get; set; }
    public ICollection<RoomParticipant> RoomParticipants { get; set; } = new List<RoomParticipant>();
    public ICollection<RoomInvitation> RoomInvitations { get; set; } = new List<RoomInvitation>();
    public Booking? Booking { get; set; }
    public ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();
    public ICollection<MatchFeedback> MatchFeedbacks { get; set; } = new List<MatchFeedback>();
    public ICollection<EloHistory> EloHistories { get; set; } = new List<EloHistory>();
    public ICollection<PlayerReport> PlayerReports { get; set; } = new List<PlayerReport>();
}
