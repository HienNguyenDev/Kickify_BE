using Kickify.Domain.Entities;
using Kickify.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kickify.Infrastructure.Configurations;

public class VenueFeedbackConfiguration : IEntityTypeConfiguration<VenueFeedback>
{
    public void Configure(EntityTypeBuilder<VenueFeedback> builder)
    {
        builder.ToTable("VenueFeedbacks", Schemas.Venue);

        builder.HasKey(f => f.VenueFeedbackId);

        builder.Property(f => f.Message)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(f => f.Rating)
            .IsRequired();

        builder.Property(f => f.CreatedAt)
            .HasColumnType("timestamp");

        builder.HasOne(f => f.Venue)
            .WithMany()
            .HasForeignKey(f => f.VenueId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(f => f.Sender)
            .WithMany()
            .HasForeignKey(f => f.SenderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
