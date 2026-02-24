using Kickify.Domain.Entities;
using Kickify.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kickify.Infrastructure.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications", Schemas.Identity);

        builder.HasKey(n => n.NotificationId);

        builder.Property(n => n.NotificationId)
            .IsRequired();

        builder.Property(n => n.UserId)
            .IsRequired();

        builder.Property(n => n.SenderId);

        builder.Property(n => n.NotificationType)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(n => n.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(n => n.Message)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(n => n.DeepLink)
            .HasMaxLength(500);

        builder.Property(n => n.IsRead)
            .HasDefaultValue(false);

        builder.Property(n => n.ReadAt)
            .HasColumnType("timestamp");

        // Relationships
        builder.HasOne(n => n.User)
            .WithMany()
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(n => n.Sender)
            .WithMany()
            .HasForeignKey(n => n.SenderId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(n => n.UserId);
        builder.HasIndex(n => n.SenderId);
        builder.HasIndex(n => n.IsRead);
        builder.HasIndex(n => n.CreatedAt);
    }
}
