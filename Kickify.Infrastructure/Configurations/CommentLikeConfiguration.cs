using Kickify.Domain.Entities;
using Kickify.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kickify.Infrastructure.Configurations;

public class CommentLikeConfiguration : IEntityTypeConfiguration<CommentLike>
{
    public void Configure(EntityTypeBuilder<CommentLike> builder)
    {
        builder.ToTable("CommentLikes", Schemas.Social);

        builder.HasKey(cl => cl.LikeId);

        builder.Property(cl => cl.LikeId)
            .IsRequired();

        builder.Property(cl => cl.CommentId)
            .IsRequired();

        builder.Property(cl => cl.UserId)
            .IsRequired();

        // Indexes
        builder.HasIndex(cl => new { cl.CommentId, cl.UserId })
            .IsUnique();

        builder.HasIndex(cl => cl.CommentId);
        builder.HasIndex(cl => cl.UserId);
    }
}
