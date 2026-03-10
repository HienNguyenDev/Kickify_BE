using Kickify.Domain.Entities;
using Kickify.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kickify.Infrastructure.Configurations;

public class VenueConfiguration : IEntityTypeConfiguration<Venue>
{
    public void Configure(EntityTypeBuilder<Venue> builder)
    {
        builder.ToTable("Venues", Schemas.Venue);

        builder.HasKey(v => v.VenueId);

        builder.Property(v => v.VenueId)
            .IsRequired();

        builder.Property(v => v.OwnerId)
            .IsRequired();

        builder.Property(v => v.VenueName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(v => v.Address)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(v => v.Latitude)
            .HasColumnType("decimal(10,8)");

        builder.Property(v => v.Longitude)
            .HasColumnType("decimal(11,8)");

        builder.Property(v => v.ContactPhone)
            .HasMaxLength(20);

        builder.Property(v => v.ContactEmail)
            .HasMaxLength(255);

        builder.Property(v => v.Description)
            .HasColumnType("text");

        builder.Property(v => v.Amenities)
            .HasColumnType("text")
            .HasComment("JSON: parking, shower, etc.");

        builder.Property(v => v.Status)
            .HasConversion<string>()
            .HasDefaultValue(Domain.Enums.VenueStatus.Draft);

        builder.Property(v => v.AdminNotes)
            .HasColumnType("text");

        builder.Property(v => v.AverageRating)
            .HasColumnType("decimal(3,2)")
            .HasDefaultValue(0);

        builder.Property(v => v.TotalReviews)
            .HasDefaultValue(0);

        // Indexes
        builder.HasIndex(v => v.OwnerId);
        builder.HasIndex(v => v.Status);

        // Relationships
        builder.HasMany(v => v.VenuePhotos)
            .WithOne(vp => vp.Venue)
            .HasForeignKey(vp => vp.VenueId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(v => v.VenueOperatingHours)
            .WithOne(voh => voh.Venue)
            .HasForeignKey(voh => voh.VenueId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(v => v.Fields)
            .WithOne(f => f.Venue)
            .HasForeignKey(f => f.VenueId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(v => v.VenueReviews)
            .WithOne(vr => vr.Venue)
            .HasForeignKey(vr => vr.VenueId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
