using Kickify.Domain.Entities;
using Kickify.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kickify.Infrastructure.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("Bookings", Schemas.Venue);

        builder.HasKey(b => b.BookingId);

        builder.Property(b => b.BookingId)
            .IsRequired();

        builder.Property(b => b.RoomId)
            .IsRequired();

        builder.Property(b => b.FieldId)
            .IsRequired();

        builder.Property(b => b.BookingDate)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(b => b.StartTime)
            .HasColumnType("time")
            .IsRequired();

        builder.Property(b => b.EndTime)
            .HasColumnType("time")
            .IsRequired();

        builder.Property(b => b.TotalAmount)
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        builder.Property(b => b.PlatformFee)
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        builder.Property(b => b.VenueAmount)
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        builder.Property(b => b.Status)
            .HasConversion<string>()
            .HasDefaultValue(Domain.Enums.BookingStatus.Confirmed);

        builder.Property(b => b.PaymentMethod)
            .HasMaxLength(50);

        builder.Property(b => b.TransactionReference)
            .HasMaxLength(100);

        // Indexes
        builder.HasIndex(b => b.RoomId);
        builder.HasIndex(b => b.FieldId);
        builder.HasIndex(b => b.BookingDate);

        // Relationships
        builder.HasMany(b => b.VenueReviews)
            .WithOne(vr => vr.Booking)
            .HasForeignKey(vr => vr.BookingId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
