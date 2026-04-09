using Kickify.Domain.Common;

namespace Kickify.Domain.Entities;

public class PlayerProfile : BaseEntity
{
    public Guid ProfileId { get; set; }
    public Guid UserId { get; set; }
    public int CurrentElo { get; set; } = 1000;
    public string CurrentRank { get; set; } = "Amateur";
    public bool IsLegend { get; set; } = false;
    public int TrustScore { get; set; } = 100;
    public int TotalMatches { get; set; } = 0;
    public int Wins { get; set; } = 0;
    public int Losses { get; set; } = 0;
    public int Draws { get; set; } = 0;
    public int MvpCount { get; set; } = 0;
    public int WinStreak { get; set; } = 0;
    public int MaxWinStreak { get; set; } = 0;
    public int ReportCount { get; set; } = 0;

    // Navigation properties
    public User User { get; set; } = null!;
}
