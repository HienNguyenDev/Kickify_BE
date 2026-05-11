namespace Kickify.Domain.Entities;

public class EloHistory
{
    public Guid EloHistoryId { get; set; }
    public Guid UserId { get; set; }
    public Guid MatchId { get; set; }
    public int EloBefore { get; set; }
    public int EloAfter { get; set; }
    public int EloChange { get; set; }
    public decimal? K1MatchResultComponent { get; set; }
    public decimal? K2FeedbackSentimentComponent { get; set; }
    public decimal? K3WinStreakComponent { get; set; }
    public decimal? K4ContributionComponent { get; set; }
    public decimal? K5TrustComponent { get; set; }
    public string? CalculationDetails { get; set; } // JSON with all coefficients
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public MatchRoom Match { get; set; } = null!;
}
