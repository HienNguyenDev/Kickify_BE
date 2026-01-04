using Kickify.Domain.Common;
using Kickify.Domain.Enums;

namespace Kickify.Domain.Entities;

public class Achievement : BaseEntity
{
    public Guid AchievementId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? BadgeIconUrl { get; set; }
    public CriteriaType CriteriaType { get; set; }
    public int CriteriaValue { get; set; }

    // Navigation properties
    public ICollection<PlayerAchievement> PlayerAchievements { get; set; } = new List<PlayerAchievement>();
}
