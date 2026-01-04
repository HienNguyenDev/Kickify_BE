using Kickify.Domain.Enums;

namespace Kickify.Domain.Entities;

public class MatchFeedback
{
    public Guid FeedbackId { get; set; }
    public Guid MatchId { get; set; }
    public Guid ReviewerId { get; set; }
    public Guid RevieweeId { get; set; }
    public int TeamworkRating { get; set; } // 1-5
    public int FairplayRating { get; set; } // 1-5
    public int AttackRating { get; set; } // 1-5
    public int DefenseRating { get; set; } // 1-5
    public int CommunicationRating { get; set; } // 1-5
    public decimal AverageRating { get; set; }
    public string? Comment { get; set; }
    public decimal? SentimentScore { get; set; } // AI-analyzed: -1.00 to 1.00
    public SentimentLabel? SentimentLabel { get; set; }
    public string? RevieweeResponse { get; set; }
    public DateTime? ResponseDate { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public MatchRoom Match { get; set; } = null!;
    public User Reviewer { get; set; } = null!;
    public User Reviewee { get; set; } = null!;
}
