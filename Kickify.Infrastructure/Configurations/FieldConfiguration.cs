using Kickify.Domain.Entities;
using Kickify.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kickify.Infrastructure.Configurations;

public class FieldConfiguration : IEntityTypeConfiguration<Field>
{
    public void Configure(EntityTypeBuilder<Field> builder)
    {
        builder.ToTable("Fields", Schemas.Venue);

        builder.HasKey(f => f.FieldId);

        builder.Property(f => f.FieldId)
            .IsRequired();

        builder.Property(f => f.VenueId)
            .IsRequired();

        builder.Property(f => f.FieldName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(f => f.FieldType)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(f => f.SurfaceType)
            .HasMaxLength(50)
            .HasComment("Grass, Artificial, etc.");

        builder.Property(f => f.HourlyRate)
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        builder.Property(f => f.PeakHourSurcharge)
            .HasColumnType("decimal(10,2)")
            .HasDefaultValue(0);

        builder.Property(f => f.IsActive)
            .HasDefaultValue(true);

        // Indexes
        builder.HasIndex(f => f.VenueId);

        // Relationships
        builder.HasMany(f => f.MatchRooms)
            .WithOne(mr => mr.Field)
            .HasForeignKey(mr => mr.FieldId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(f => f.MatchPresets)
            .WithOne(mp => mp.Field)
            .HasForeignKey(mp => mp.FieldId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(f => f.Bookings)
            .WithOne(b => b.Field)
            .HasForeignKey(b => b.FieldId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
