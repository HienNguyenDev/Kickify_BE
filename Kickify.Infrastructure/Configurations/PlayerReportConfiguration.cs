using Kickify.Domain.Entities;
using Kickify.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kickify.Infrastructure.Configurations;

public class PlayerReportConfiguration : IEntityTypeConfiguration<PlayerReport>
{
    public void Configure(EntityTypeBuilder<PlayerReport> builder)
    {
        builder.ToTable("PlayerReports", Schemas.Evaluation);

        builder.HasKey(pr => pr.ReportId);

        builder.Property(pr => pr.ReportId)
            .IsRequired();

        builder.Property(pr => pr.ReporterId)
            .IsRequired();

        builder.Property(pr => pr.ReportedId)
            .IsRequired();

        builder.Property(pr => pr.MatchId);

        builder.Property(pr => pr.ReportType)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(pr => pr.Description)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(pr => pr.EvidenceUrls)
            .HasColumnType("text")
            .HasComment("JSON array of screenshot URLs");

        builder.Property(pr => pr.Status)
            .HasConversion<string>()
            .HasDefaultValue(Domain.Enums.ReportStatus.Pending);

        builder.Property(pr => pr.AdminNotes)
            .HasColumnType("text");

        builder.Property(pr => pr.ActionTaken)
            .HasMaxLength(255);

        builder.Property(pr => pr.ResolvedBy);

        builder.Property(pr => pr.ResolvedAt)
            .HasColumnType("timestamp");

        // Indexes
        builder.HasIndex(pr => pr.ReportedId);
        builder.HasIndex(pr => pr.Status);
    }
}
