using Kickify.Domain.Entities;
using Kickify.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kickify.Infrastructure.Configurations;

public class PlayerAchievementConfiguration : IEntityTypeConfiguration<PlayerAchievement>
{
    public void Configure(EntityTypeBuilder<PlayerAchievement> builder)
    {
        builder.ToTable("PlayerAchievements", Schemas.Evaluation);

        builder.HasKey(pa => pa.PlayerAchievementId);

        builder.Property(pa => pa.PlayerAchievementId)
            .IsRequired();

        builder.Property(pa => pa.UserId)
            .IsRequired();

        builder.Property(pa => pa.AchievementId)
            .IsRequired();

        // Indexes
        builder.HasIndex(pa => new { pa.UserId, pa.AchievementId })
            .IsUnique();
    }
}
