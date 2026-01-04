using Kickify.Domain.Enums;

namespace Kickify.Domain.Entities;

public class RoomInvitation
{
    public Guid InvitationId { get; set; }
    public Guid RoomId { get; set; }
    public Guid InviterId { get; set; }
    public Guid InviteeId { get; set; }
    public string? InvitationLink { get; set; }
    public string? QrCodeUrl { get; set; }
    public InvitationStatus Status { get; set; } = InvitationStatus.Pending;
    public DateTime CreatedAt { get; set; }
    public DateTime? RespondedAt { get; set; }

    // Navigation properties
    public MatchRoom MatchRoom { get; set; } = null!;
    public User Inviter { get; set; } = null!;
    public User Invitee { get; set; } = null!;
}
