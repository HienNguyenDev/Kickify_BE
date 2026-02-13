using Kickify.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kickify.Infrastructure.Configurations;

public class MatchResultVoteConfiguration : IEntityTypeConfiguration<MatchResultVote>
{
    public void Configure(EntityTypeBuilder<MatchResultVote> builder)
    {
        builder.ToTable("match_result_votes", "match");

        builder.HasKey(v => v.VoteId);

        builder.Property(v => v.VoteId)
            .HasColumnName("vote_id");

        builder.Property(v => v.RoomId)
            .HasColumnName("room_id")
            .IsRequired();

        builder.Property(v => v.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(v => v.Vote)
            .HasColumnName("vote")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(v => v.VotedAt)
            .HasColumnName("voted_at")
            .IsRequired();

        // Unique constraint: one vote per user per room
        builder.HasIndex(v => new { v.RoomId, v.UserId })
            .IsUnique();

        // Relationships
        builder.HasOne(v => v.MatchRoom)
            .WithMany(r => r.ResultVotes)
            .HasForeignKey(v => v.RoomId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(v => v.User)
            .WithMany()
            .HasForeignKey(v => v.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
