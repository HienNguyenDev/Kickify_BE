using Kickify.Domain.Entities;
using Kickify.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kickify.Infrastructure.Configurations;

public class AnnouncementConfiguration : IEntityTypeConfiguration<Announcement>
{
    public void Configure(EntityTypeBuilder<Announcement> builder)
    {
        builder.ToTable("Announcements", Schemas.System);

        builder.HasKey(a => a.AnnouncementId);

        builder.Property(a => a.AnnouncementId)
            .IsRequired();

        builder.Property(a => a.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(a => a.Content)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(a => a.AnnouncementType)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(a => a.Priority)
            .HasConversion<string>();

        builder.Property(a => a.ShowFrom)
            .HasColumnType("timestamp")
            .IsRequired();

        builder.Property(a => a.ShowTo)
            .HasColumnType("timestamp");

        builder.Property(a => a.IsActive)
            .HasDefaultValue(true);

        builder.Property(a => a.CreatedBy)
            .IsRequired();

        builder.HasOne(a => a.Creator)
            .WithMany()
            .HasForeignKey(a => a.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(a => a.IsActive);
        builder.HasIndex(a => a.ShowFrom);
    }
}
