namespace Kickify.Domain.Entities;

public class EloHistory
{
    public Guid EloHistoryId { get; set; }
    public Guid UserId { get; set; }
    public Guid MatchId { get; set; }
    public int EloBefore { get; set; }
    public int EloAfter { get; set; }
    public int EloChange { get; set; }
    public decimal? WinLossComponent { get; set; }
    public decimal? PerformanceComponent { get; set; }
    public decimal? FeedbackComponent { get; set; }
    public decimal? SentimentComponent { get; set; }
    public decimal? TrustComponent { get; set; }
    public decimal? RoleComponent { get; set; }
    public string? CalculationDetails { get; set; } // JSON with all coefficients
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public MatchRoom Match { get; set; } = null!;
}
