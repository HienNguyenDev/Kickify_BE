namespace Kickify.Domain.Entities;

public class PlayerRadarSnapshot
{
    public Guid PlayerId { get; set; }
    public decimal Form { get; set; }
    public decimal WinRate { get; set; }
    public decimal CommunityScore { get; set; }
    public decimal Trust { get; set; }
    public decimal Contribution { get; set; }
    public string AssessmentsJson { get; set; } = "[]";
    public string Summary { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }

    public User Player { get; set; } = null!;
}
