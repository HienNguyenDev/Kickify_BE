using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kickify.Infrastructure.Configurations;

public class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        builder.ToTable("Posts", Schemas.Social);

        builder.HasKey(p => p.PostId);

        builder.Property(p => p.PostId)
            .IsRequired();

        builder.Property(p => p.UserId)
            .IsRequired();

        builder.Property(p => p.Content)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(p => p.TotalMedia)
            .HasDefaultValue(0);

        builder.Property(p => p.TotalLikes)
            .HasDefaultValue(0);

        builder.Property(p => p.TotalComments)
            .HasDefaultValue(0);

        builder.Property(p => p.Visibility)
            .HasConversion<string>()
            .HasDefaultValue(PostVisibility.Public);

        builder.Property(p => p.IsEdited)
            .HasDefaultValue(false);

        builder.Property(p => p.EditedAt)
            .HasColumnType("timestamp with time zone");

        builder.Property(p => p.IsActive)
            .HasDefaultValue(true);

        // Indexes
        builder.HasIndex(p => p.UserId);
        builder.HasIndex(p => p.CreatedAt);
        builder.HasIndex(p => p.IsActive);

        // Relationships
        builder.HasMany(p => p.PostMedia)
            .WithOne(pm => pm.Post)
            .HasForeignKey(pm => pm.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.PostLikes)
            .WithOne(pl => pl.Post)
            .HasForeignKey(pl => pl.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Comments)
            .WithOne(c => c.Post)
            .HasForeignKey(c => c.PostId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
