using Kickify.Domain.Entities;
using Kickify.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kickify.Infrastructure.Configurations;

public class AchievementConfiguration : IEntityTypeConfiguration<Achievement>
{
    public void Configure(EntityTypeBuilder<Achievement> builder)
    {
        builder.ToTable("Achievements", Schemas.Evaluation);

        builder.HasKey(a => a.AchievementId);

        builder.Property(a => a.AchievementId)
            .IsRequired();

        builder.Property(a => a.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(a => a.Description)
            .HasColumnType("text");

        builder.Property(a => a.BadgeIconUrl)
            .HasMaxLength(500);

        builder.Property(a => a.CriteriaType)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(a => a.CriteriaValue)
            .IsRequired();

        // Relationships
        builder.HasMany(a => a.PlayerAchievements)
            .WithOne(pa => pa.Achievement)
            .HasForeignKey(pa => pa.AchievementId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
