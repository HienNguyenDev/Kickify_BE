namespace Kickify.Domain.Entities;

public class PlayerAchievement
{
    public Guid PlayerAchievementId { get; set; }
    public Guid UserId { get; set; }
    public Guid AchievementId { get; set; }
    public DateTime EarnedAt { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public Achievement Achievement { get; set; } = null!;
}
