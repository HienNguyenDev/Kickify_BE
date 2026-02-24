using Kickify.Domain.Entities;
using Kickify.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kickify.Infrastructure.Configurations;

public class PlayerProfileConfiguration : IEntityTypeConfiguration<PlayerProfile>
{
    public void Configure(EntityTypeBuilder<PlayerProfile> builder)
    {
        builder.ToTable("PlayerProfiles", Schemas.Identity);

        builder.HasKey(p => p.ProfileId);

        builder.Property(p => p.ProfileId)
            .IsRequired();

        builder.Property(pp => pp.UserId)
            .IsRequired();

        builder.Property(p => p.CurrentElo)
            .HasDefaultValue(1000);

        builder.Property(p => p.TrustScore)
            .HasDefaultValue(100);

        builder.Property(p => p.TotalMatches)
            .HasDefaultValue(0);

        builder.Property(p => p.Wins)
            .HasDefaultValue(0);

        builder.Property(p => p.Losses)
            .HasDefaultValue(0);

        builder.Property(p => p.Draws)
            .HasDefaultValue(0);

        builder.Property(p => p.MvpCount)
            .HasDefaultValue(0);

        builder.Property(p => p.WinStreak)
            .HasDefaultValue(0);

        builder.Property(p => p.MaxWinStreak)
            .HasDefaultValue(0);

        builder.Property(p => p.ReportCount)
            .HasDefaultValue(0);
    }
}
