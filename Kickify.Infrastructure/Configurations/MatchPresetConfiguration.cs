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

        builder.Property(mp => mp.RoomName)
            .HasColumnName("PresetName")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(mp => mp.FieldId);

        builder.Property(mp => mp.MatchFormat)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(mp => mp.Visibility)
            .HasConversion<string>()
            .HasDefaultValue(Domain.Enums.Visibility.Public)
            .IsRequired();

        builder.Property(mp => mp.Password)
            .HasColumnName("RoomPassword")
            .HasColumnType("text");

        builder.Property(mp => mp.StartTime)
            .HasColumnType("time")
            .IsRequired();

        builder.Property(mp => mp.Rules)
            .HasColumnType("text");

        builder.Property(mp => mp.DurationMinutes)
            .IsRequired();

        builder.Property(mp => mp.Description)
            .HasColumnType("text");

        // Indexes
        builder.HasIndex(mp => mp.UserId);
        builder.HasIndex(mp => mp.FieldId);
    }
}
