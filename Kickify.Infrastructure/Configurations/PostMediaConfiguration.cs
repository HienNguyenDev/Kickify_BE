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

        builder.HasKey(pm => pm.MediaId);

        builder.Property(pm => pm.MediaId)
            .IsRequired();

        builder.Property(pm => pm.PostId)
            .IsRequired();

        builder.Property(pm => pm.MediaUrl)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(pm => pm.MediaType)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(pm => pm.ThumbnailUrl)
            .HasMaxLength(500)
            .HasComment("For videos");

        builder.Property(pm => pm.FileSize)
            .HasComment("In bytes");

        builder.Property(pm => pm.Duration)
            .HasComment("For videos (seconds)");

        builder.Property(pm => pm.Width)
            .HasComment("For images/videos");

        builder.Property(pm => pm.Height)
            .HasComment("For images/videos");

        builder.Property(pm => pm.DisplayOrder)
            .HasDefaultValue(0);

        // Indexes
        builder.HasIndex(pm => pm.PostId);
        builder.HasIndex(pm => pm.DisplayOrder);
    }
}
