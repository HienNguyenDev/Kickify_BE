using Kickify.Domain.Enums;

namespace Kickify.Domain.Entities;

public class AfkVote
{
    public Guid Id { get; set; }
    public Guid MatchRoomId { get; set; }
    public Guid VoterId { get; set; }
    public Guid TargetPlayerId { get; set; }
    public TeamAssignment Team { get; set; }
    public DateTime CreatedAt { get; set; }

    public MatchRoom MatchRoom { get; set; } = null!;
    public User Voter { get; set; } = null!;
    public User TargetPlayer { get; set; } = null!;
}
