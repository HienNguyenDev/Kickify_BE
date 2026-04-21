using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Infrastructure.Database;

namespace Kickify.Api.Extensions;

public static class SeedingExtensions
{
    public static void SeedData(this IApplicationBuilder app)
    {
        using IServiceScope scope = app.ApplicationServices.CreateScope();
        using ApplicationDbContext dbContext =
            scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        SeedAchievements(dbContext);
    }

    private static void SeedAchievements(ApplicationDbContext dbContext)
    {
        if (dbContext.Achievements.Any())
            return;

        var achievements = new List<Achievement>
        {
            new()
            {
                AchievementId = Guid.NewGuid(),
                Name = "First Match",
                Description = "Complete your first match.",
                CriteriaType = CriteriaType.Matches,
                CriteriaValue = 1,
                BadgeIconUrl = "https://via.placeholder.com/128?text=FirstMatch"
            },
            new()
            {
                AchievementId = Guid.NewGuid(),
                Name = "Veteran Player",
                Description = "Complete 50 matches.",
                CriteriaType = CriteriaType.Matches,
                CriteriaValue = 50,
                BadgeIconUrl = "https://via.placeholder.com/128?text=Veteran"
            },
            new()
            {
                AchievementId = Guid.NewGuid(),
                Name = "On Fire",
                Description = "Win 3 matches in a row.",
                CriteriaType = CriteriaType.WinStreak,
                CriteriaValue = 3,
                BadgeIconUrl = "https://via.placeholder.com/128?text=OnFire"
            },
            new()
            {
                AchievementId = Guid.NewGuid(),
                Name = "Unstoppable",
                Description = "Win 10 matches in a row.",
                CriteriaType = CriteriaType.WinStreak,
                CriteriaValue = 10,
                BadgeIconUrl = "https://via.placeholder.com/128?text=Unstoppable"
            },
            new()
            {
                AchievementId = Guid.NewGuid(),
                Name = "Crowd Favorite",
                Description = "Receive 10 feedbacks from other players.",
                CriteriaType = CriteriaType.ReceivedFeedback,
                CriteriaValue = 10,
                BadgeIconUrl = "https://via.placeholder.com/128?text=CrowdFav"
            },
            new()
            {
                AchievementId = Guid.NewGuid(),
                Name = "Superstar",
                Description = "Give feedback to 50 different players.",
                CriteriaType = CriteriaType.Feedback,
                CriteriaValue = 50,
                BadgeIconUrl = "https://via.placeholder.com/128?text=Superstar"
            },
            new()
            {
                AchievementId = Guid.NewGuid(),
                Name = "Fair Play Spirit",
                Description = "Maintain a fair play score above 90 for 20 matches.",
                CriteriaType = CriteriaType.Fairplay,
                CriteriaValue = 20,
                BadgeIconUrl = "https://via.placeholder.com/128?text=FairPlay"
            }
        };

        dbContext.Achievements.AddRange(achievements);
        dbContext.SaveChanges();
    }
}
