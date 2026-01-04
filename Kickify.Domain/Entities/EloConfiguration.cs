using Kickify.Domain.Common;

namespace Kickify.Domain.Entities;

public class EloConfiguration : BaseEntity
{
    public Guid ConfigId { get; set; }
    public string VersionName { get; set; } = string.Empty;
    public decimal KBase { get; set; } // Base K factor
    public decimal KWinloss { get; set; }
    public decimal KPerformance { get; set; }
    public decimal KFeedback { get; set; }
    public decimal KSentiment { get; set; }
    public decimal KTrust { get; set; }
    public decimal KRole { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool IsActive { get; set; } = false;
    public Guid? CreatedBy { get; set; }

    // Navigation properties
    public User? Creator { get; set; }
}
