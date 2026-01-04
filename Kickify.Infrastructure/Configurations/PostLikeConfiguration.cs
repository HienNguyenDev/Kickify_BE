using Kickify.Domain.Entities;
using Kickify.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kickify.Infrastructure.Configurations;

public class PostLikeConfiguration : IEntityTypeConfiguration<PostLike>
{
    public void Configure(EntityTypeBuilder<PostLike> builder)
    {
        builder.ToTable("PostLikes", Schemas.Social);

        builder.HasKey(pl => pl.LikeId);

        builder.Property(pl => pl.LikeId)
            .IsRequired();

        builder.Property(pl => pl.PostId)
            .IsRequired();

        builder.Property(pl => pl.UserId)
            .IsRequired();

        // Indexes
        builder.HasIndex(pl => new { pl.PostId, pl.UserId })
            .IsUnique();

        builder.HasIndex(pl => pl.PostId);
        builder.HasIndex(pl => pl.UserId);
    }
}
