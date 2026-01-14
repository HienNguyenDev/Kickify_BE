using Kickify.Domain.Entities;
using Kickify.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kickify.Infrastructure.Configurations;

public class PostMediaConfiguration : IEntityTypeConfiguration<PostMedia>
{
    public void Configure(EntityTypeBuilder<PostMedia> builder)
    {
        builder.ToTable("PostMedia", Schemas.Social);

        // Primary Key
        builder.HasKey(pm => pm.MediaId);

        // Properties
        builder.Property(pm => pm.MediaId)
            .IsRequired();

        builder.Property(pm => pm.PostId)
            .IsRequired();

        builder.Property(pm => pm.FileName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(pm => pm.StoragePath)
            .HasMaxLength(500)
            .IsRequired()
            .HasComment("MinIO object path (e.g., images/2024/01/15/abc123.jpg)");

        builder.Property(pm => pm.PublicUrl)
            .HasMaxLength(1000)
            .IsRequired()
            .HasComment("Full CDN URL for client access");

        builder.Property(pm => pm.ContentType)
            .HasMaxLength(100)
            .IsRequired()
            .HasComment("MIME type (e.g., image/jpeg, video/mp4)");

        builder.Property(pm => pm.BucketName)
            .HasMaxLength(100)
            .IsRequired()
            .HasDefaultValue("kickify-media")
            .HasComment("MinIO bucket name");

        builder.Property(pm => pm.MediaType)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(pm => pm.FileSize)
            .IsRequired()
            .HasComment("File size in bytes");

        builder.Property(pm => pm.ThumbnailStoragePath)
            .HasMaxLength(500)
            .HasComment("MinIO path for video thumbnail");

        builder.Property(pm => pm.ThumbnailUrl)
            .HasMaxLength(1000)
            .HasComment("Full CDN URL for video thumbnail");

        builder.Property(pm => pm.Duration)
            .HasComment("Video duration in seconds");

        builder.Property(pm => pm.Width)
            .HasComment("Media width in pixels");

        builder.Property(pm => pm.Height)
            .HasComment("Media height in pixels");

        builder.Property(pm => pm.DisplayOrder)
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(pm => pm.IsProcessed)
            .HasDefaultValue(true)
            .IsRequired()
            .HasComment("Processing status for async video encoding");

        // Indexes
        builder.HasIndex(pm => pm.PostId)
            .HasDatabaseName("IX_PostMedia_PostId");

        builder.HasIndex(pm => pm.StoragePath)
            .HasDatabaseName("IX_PostMedia_StoragePath");

        builder.HasIndex(pm => new { pm.PostId, pm.DisplayOrder })
            .HasDatabaseName("IX_PostMedia_PostId_DisplayOrder");

        builder.HasIndex(pm => pm.MediaType)
            .HasDatabaseName("IX_PostMedia_MediaType");

        // Xóa HasFilter vì gây l?i v?i EF Core PostgreSQL migration generation
        builder.HasIndex(pm => pm.IsProcessed)
            .HasDatabaseName("IX_PostMedia_IsProcessed");

        // Relationships
        builder.HasOne(pm => pm.Post)
            .WithMany(p => p.PostMedia)
            .HasForeignKey(pm => pm.PostId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
