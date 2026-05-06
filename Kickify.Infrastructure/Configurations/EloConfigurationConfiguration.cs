using Kickify.Domain.Entities;
using Kickify.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kickify.Infrastructure.Configurations;

public class EloConfigurationConfiguration : IEntityTypeConfiguration<EloConfiguration>
{
    public void Configure(EntityTypeBuilder<EloConfiguration> builder)
    {
        builder.ToTable("EloConfigurations", Schemas.Evaluation);

        builder.HasKey(ec => ec.ConfigId);

        builder.Property(ec => ec.ConfigId)
            .IsRequired();

        builder.Property(ec => ec.VersionName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(ec => ec.K1MatchResult)
            .HasColumnType("decimal(5,2)")
            .IsRequired()
            .HasComment("K1 for result - expected score");

        builder.Property(ec => ec.K2FeedbackSentiment)
            .HasColumnType("decimal(5,2)")
            .IsRequired();

        builder.Property(ec => ec.K3WinStreak)
            .HasColumnType("decimal(5,2)")
            .IsRequired();

        builder.Property(ec => ec.K4Contribution)
            .HasColumnType("decimal(5,2)")
            .IsRequired();

        builder.Property(ec => ec.K5Trust)
            .HasColumnType("decimal(5,2)")
            .IsRequired();

        builder.Property(ec => ec.EffectiveFrom)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(ec => ec.EffectiveTo)
            .HasColumnType("date");

        builder.Property(ec => ec.IsActive)
            .HasDefaultValue(false);

        builder.Property(ec => ec.CreatedBy);

        // Indexes
        builder.HasIndex(ec => ec.EffectiveFrom);
        builder.HasIndex(ec => ec.IsActive)
            .IsUnique()
            .HasFilter("\"IsActive\" = true");
    }
}
