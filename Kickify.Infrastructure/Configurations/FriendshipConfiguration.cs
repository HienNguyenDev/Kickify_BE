using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kickify.Infrastructure.Configurations;

public class FriendshipConfiguration : IEntityTypeConfiguration<Friendship>
{
    public void Configure(EntityTypeBuilder<Friendship> builder)
    {
        builder.ToTable("Friendships", Schemas.Social);
        builder.HasKey(f => f.FriendshipId);

        builder.Property(f => f.Status).HasConversion<string>().HasDefaultValue(FriendshipStatus.Pending).IsRequired();
        builder.Property(f => f.RespondedAt).IsRequired(false);

        builder.HasOne(f => f.Requester).WithMany().HasForeignKey(f => f.RequesterId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(f => f.Addressee).WithMany().HasForeignKey(f => f.AddresseeId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(f => new { f.RequesterId, f.AddresseeId }).IsUnique();
        builder.HasIndex(f => f.AddresseeId);
        builder.HasIndex(f => f.Status);
    }
}
