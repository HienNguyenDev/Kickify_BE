using Kickify.Domain.Entities;
using Kickify.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kickify.Infrastructure.Configurations;

public class VenueOperatingHourConfiguration : IEntityTypeConfiguration<VenueOperatingHour>
{
    public void Configure(EntityTypeBuilder<VenueOperatingHour> builder)
    {
        builder.ToTable("VenueOperatingHours", Schemas.Venue);

        builder.HasKey(voh => voh.HoursId);

        builder.Property(voh => voh.HoursId)
            .IsRequired();

        builder.Property(voh => voh.VenueId)
            .IsRequired();

        builder.Property(voh => voh.DayOfWeek)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(voh => voh.OpenTime)
            .HasColumnType("time");

        builder.Property(voh => voh.CloseTime)
            .HasColumnType("time");

        builder.Property(voh => voh.IsClosed)
            .HasDefaultValue(false);

        // Indexes
        builder.HasIndex(voh => new { voh.VenueId, voh.DayOfWeek })
            .IsUnique();
    }
}
