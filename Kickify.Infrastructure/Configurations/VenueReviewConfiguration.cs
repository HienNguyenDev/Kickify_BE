using Kickify.Domain.Entities;
using Kickify.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kickify.Infrastructure.Configurations;

public class VenueReviewConfiguration : IEntityTypeConfiguration<VenueReview>
{
    public void Configure(EntityTypeBuilder<VenueReview> builder)
    {
        builder.ToTable("VenueReviews", Schemas.Venue);

        builder.HasKey(vr => vr.ReviewId);

        builder.Property(vr => vr.ReviewId)
            .IsRequired();

        builder.Property(vr => vr.VenueId)
            .IsRequired();

        builder.Property(vr => vr.UserId)
            .IsRequired();

        builder.Property(vr => vr.BookingId)
            .IsRequired();

        builder.Property(vr => vr.Rating)
            .IsRequired()
            .HasComment("1-5");

        builder.Property(vr => vr.Comment)
            .HasColumnType("text");

        builder.Property(vr => vr.OwnerResponse)
            .HasColumnType("text");

        builder.Property(vr => vr.ResponseDate)
            .HasColumnType("timestamp");

        // Indexes
        builder.HasIndex(vr => vr.VenueId);
        builder.HasIndex(vr => new { vr.UserId, vr.BookingId })
            .IsUnique();
    }
}
