using Kickify.Domain.Common;

namespace Kickify.Domain.Entities;

public class EloConfiguration : BaseEntity
{
    public Guid ConfigId { get; set; }
    public string VersionName { get; set; } = string.Empty;
    public decimal K1MatchResult { get; set; }
    public decimal K2FeedbackSentiment { get; set; }
    public decimal K3WinStreak { get; set; }
    public decimal K4Contribution { get; set; }
    public decimal K5Trust { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool IsActive { get; set; } = false;
    public Guid? CreatedBy { get; set; }

    // Navigation properties
    public User? Creator { get; set; }
}
