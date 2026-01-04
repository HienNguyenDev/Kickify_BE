using Kickify.Domain.Entities;
using Kickify.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kickify.Infrastructure.Configurations;

public class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.ToTable("Comments", Schemas.Social);

        builder.HasKey(c => c.CommentId);

        builder.Property(c => c.CommentId)
            .IsRequired();

        builder.Property(c => c.PostId)
            .IsRequired();

        builder.Property(c => c.UserId)
            .IsRequired();

        builder.Property(c => c.ParentCommentId)
            .HasComment("NULL = root comment, NOT NULL = reply");

        builder.Property(c => c.Content)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(c => c.TotalLikes)
            .HasDefaultValue(0);

        builder.Property(c => c.TotalReplies)
            .HasDefaultValue(0)
            .HasComment("Only count for root comments");

        builder.Property(c => c.IsEdited)
            .HasDefaultValue(false);

        builder.Property(c => c.IsActive)
            .HasDefaultValue(true);

        // Indexes
        builder.HasIndex(c => c.PostId);
        builder.HasIndex(c => c.UserId);
        builder.HasIndex(c => c.ParentCommentId);
        builder.HasIndex(c => c.CreatedAt);

        // Relationships
        builder.HasOne(c => c.ParentComment)
            .WithMany(c => c.Replies)
            .HasForeignKey(c => c.ParentCommentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.CommentLikes)
            .WithOne(cl => cl.Comment)
            .HasForeignKey(cl => cl.CommentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
