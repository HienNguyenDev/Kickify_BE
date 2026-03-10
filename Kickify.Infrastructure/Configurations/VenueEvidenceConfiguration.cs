using Kickify.Domain.Entities;
using Kickify.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kickify.Infrastructure.Configurations;

public class VenueEvidenceConfiguration : IEntityTypeConfiguration<VenueEvidence>
{
    public void Configure(EntityTypeBuilder<VenueEvidence> builder)
    {
        builder.ToTable("VenueEvidences", Schemas.Venue);

        builder.HasKey(e => e.EvidenceId);

        builder.Property(e => e.EvidenceId).IsRequired();
        builder.Property(e => e.VenueId).IsRequired();

        builder.Property(e => e.FileUrl)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(e => e.FileName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(e => e.ContentType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.FileSize).IsRequired();

        builder.Property(e => e.CreatedAt)
            .HasColumnType("timestamp");

        builder.HasOne(e => e.Venue)
            .WithMany(v => v.VenueEvidences)
            .HasForeignKey(e => e.VenueId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.VenueId);
    }
}
