using Kickify.Domain.Entities;
using Kickify.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kickify.Infrastructure.Configurations;

public class MatchFeedbackConfiguration : IEntityTypeConfiguration<MatchFeedback>
{
    public void Configure(EntityTypeBuilder<MatchFeedback> builder)
    {
        builder.ToTable("MatchFeedbacks", Schemas.Evaluation);

        builder.HasKey(mf => mf.FeedbackId);

        builder.Property(mf => mf.FeedbackId)
            .IsRequired();

        builder.Property(mf => mf.MatchId)
            .IsRequired();

        builder.Property(mf => mf.ReviewerId)
            .IsRequired();

        builder.Property(mf => mf.RevieweeId)
            .IsRequired();

        builder.Property(mf => mf.Rating)
            .IsRequired()
            .HasComment("1-5");

        builder.Property(mf => mf.Comment)
            .HasColumnType("text");

        builder.Property(mf => mf.SentimentScore)
            .HasColumnType("decimal(5,2)")
            .HasComment("AI-analyzed: -1.00 to 1.00");

        builder.Property(mf => mf.SentimentLabel)
            .HasConversion<string>();

        builder.Property(mf => mf.RevieweeResponse)
            .HasColumnType("text");

        builder.Property(mf => mf.ResponseDate)
            .HasColumnType("timestamp with time zone");

        // One feedback row per (match, reviewer, reviewee); additional opponents via separate reviewees / requests.
        builder.HasIndex(mf => mf.MatchId);
        builder.HasIndex(mf => mf.RevieweeId);
        builder.HasIndex(mf => new { mf.MatchId, mf.ReviewerId, mf.RevieweeId })
            .IsUnique();
    }
}
