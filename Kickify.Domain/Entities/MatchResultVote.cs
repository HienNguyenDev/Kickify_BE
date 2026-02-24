using Kickify.Domain.Enums;

namespace Kickify.Domain.Entities;

public class MatchResultVote
{
    public Guid VoteId { get; set; }
    public Guid RoomId { get; set; }
    public Guid UserId { get; set; }
    public MatchResult Vote { get; set; }
    public DateTime VotedAt { get; set; }

    // Navigation properties
    public MatchRoom MatchRoom { get; set; } = null!;
    public User User { get; set; } = null!;
}
