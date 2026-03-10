using Kickify.Domain.Entities;
using Kickify.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kickify.Infrastructure.Configurations;

public class ContentReportConfiguration : IEntityTypeConfiguration<ContentReport>
{
    public void Configure(EntityTypeBuilder<ContentReport> builder)
    {
        builder.ToTable("ContentReports", Schemas.Social);

        builder.HasKey(cr => cr.ReportId);

        builder.Property(cr => cr.ReportId).IsRequired();
        builder.Property(cr => cr.ReporterId).IsRequired();
        builder.Property(cr => cr.ContentId).IsRequired();
        builder.Property(cr => cr.ContentOwnerId).IsRequired();

        builder.Property(cr => cr.ContentType)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(cr => cr.Reason)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(cr => cr.Description)
            .HasColumnType("text");

        builder.Property(cr => cr.Status)
            .HasConversion<string>()
            .HasDefaultValue(Domain.Enums.ReportStatus.Pending);

        builder.Property(cr => cr.AdminNotes)
            .HasColumnType("text");

        builder.Property(cr => cr.ResolvedAt)
            .HasColumnType("timestamp");

        builder.Property(cr => cr.CreatedAt)
            .HasColumnType("timestamp");

        builder.HasOne(cr => cr.Reporter)
            .WithMany()
            .HasForeignKey(cr => cr.ReporterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(cr => cr.ContentOwner)
            .WithMany()
            .HasForeignKey(cr => cr.ContentOwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(cr => cr.Resolver)
            .WithMany()
            .HasForeignKey(cr => cr.ResolvedBy)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(cr => cr.ContentId);
        builder.HasIndex(cr => cr.Status);
        builder.HasIndex(cr => new { cr.ReporterId, cr.ContentId }).IsUnique();
    }
}
