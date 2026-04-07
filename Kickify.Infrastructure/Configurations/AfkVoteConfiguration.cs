using Kickify.Domain.Entities;
using Kickify.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kickify.Infrastructure.Configurations;

public class AfkVoteConfiguration : IEntityTypeConfiguration<AfkVote>
{
    public void Configure(EntityTypeBuilder<AfkVote> builder)
    {
        builder.ToTable("AfkVotes", Schemas.Match);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Team)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.HasIndex(x => new { x.MatchRoomId, x.VoterId, x.TargetPlayerId })
            .IsUnique();

        builder.HasIndex(x => x.MatchRoomId);
        builder.HasIndex(x => x.TargetPlayerId);
        builder.HasIndex(x => x.VoterId);

        builder.HasOne(x => x.MatchRoom)
            .WithMany(x => x.AfkVotes)
            .HasForeignKey(x => x.MatchRoomId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Voter)
            .WithMany()
            .HasForeignKey(x => x.VoterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.TargetPlayer)
            .WithMany()
            .HasForeignKey(x => x.TargetPlayerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
