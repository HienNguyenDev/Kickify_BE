using Kickify.Domain.Entities;
using Kickify.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kickify.Infrastructure.Configurations;

public class EloHistoryConfiguration : IEntityTypeConfiguration<EloHistory>
{
    public void Configure(EntityTypeBuilder<EloHistory> builder)
    {
        builder.ToTable("EloHistories", Schemas.Evaluation);

        builder.HasKey(eh => eh.EloHistoryId);

        builder.Property(eh => eh.EloHistoryId)
            .IsRequired();

        builder.Property(eh => eh.UserId)
            .IsRequired();

        builder.Property(eh => eh.MatchId)
            .IsRequired();

        builder.Property(eh => eh.EloBefore)
            .IsRequired();

        builder.Property(eh => eh.EloAfter)
            .IsRequired();

        builder.Property(eh => eh.EloChange)
            .IsRequired();

        builder.Property(eh => eh.WinLossComponent)
            .HasColumnType("decimal(6,2)");

        builder.Property(eh => eh.PerformanceComponent)
            .HasColumnType("decimal(6,2)");

        builder.Property(eh => eh.FeedbackComponent)
            .HasColumnType("decimal(6,2)");

        builder.Property(eh => eh.SentimentComponent)
            .HasColumnType("decimal(6,2)");

        builder.Property(eh => eh.TrustComponent)
            .HasColumnType("decimal(6,2)");

        builder.Property(eh => eh.RoleComponent)
            .HasColumnType("decimal(6,2)");

        builder.Property(eh => eh.CalculationDetails)
            .HasColumnType("text")
            .HasComment("JSON with all coefficients");

        // Indexes
        builder.HasIndex(eh => eh.UserId);
        builder.HasIndex(eh => eh.MatchId);
        builder.HasIndex(eh => eh.CreatedAt);
    }
}
