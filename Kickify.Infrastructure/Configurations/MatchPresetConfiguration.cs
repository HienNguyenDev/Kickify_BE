using Kickify.Domain.Entities;
using Kickify.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kickify.Infrastructure.Configurations;

public class MatchPresetConfiguration : IEntityTypeConfiguration<MatchPreset>
{
    public void Configure(EntityTypeBuilder<MatchPreset> builder)
    {
        builder.ToTable("MatchPresets", Schemas.Match);

        builder.HasKey(mp => mp.PresetId);

        builder.Property(mp => mp.PresetId)
            .IsRequired();

        builder.Property(mp => mp.UserId)
            .IsRequired();

        builder.Property(mp => mp.PresetName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(mp => mp.FieldId);

        builder.Property(mp => mp.CustomLocation)
            .HasColumnType("text");

        builder.Property(mp => mp.MatchFormat)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(mp => mp.DurationMinutes)
            .IsRequired();

        builder.Property(mp => mp.Description)
            .HasColumnType("text");

        // Indexes
        builder.HasIndex(mp => mp.UserId);
    }
}
