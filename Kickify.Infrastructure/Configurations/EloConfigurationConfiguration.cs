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

        builder.Property(ec => ec.KBase)
            .HasColumnType("decimal(5,2)")
            .IsRequired()
            .HasComment("Base K factor");

        builder.Property(ec => ec.KWinloss)
            .HasColumnType("decimal(5,2)")
            .IsRequired();

        builder.Property(ec => ec.KPerformance)
            .HasColumnType("decimal(5,2)")
            .IsRequired();

        builder.Property(ec => ec.KFeedback)
            .HasColumnType("decimal(5,2)")
            .IsRequired();

        builder.Property(ec => ec.KSentiment)
            .HasColumnType("decimal(5,2)")
            .IsRequired();

        builder.Property(ec => ec.KTrust)
            .HasColumnType("decimal(5,2)")
            .IsRequired();

        builder.Property(ec => ec.KRole)
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
        builder.HasIndex(ec => ec.IsActive);
    }
}
