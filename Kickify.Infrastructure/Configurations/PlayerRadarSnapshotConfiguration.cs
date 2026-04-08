using Kickify.Domain.Entities;
using Kickify.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kickify.Infrastructure.Configurations;

public class PlayerRadarSnapshotConfiguration : IEntityTypeConfiguration<PlayerRadarSnapshot>
{
    public void Configure(EntityTypeBuilder<PlayerRadarSnapshot> builder)
    {
        builder.ToTable("PlayerRadarSnapshots", Schemas.Evaluation);

        builder.HasKey(x => x.PlayerId);

        builder.Property(x => x.Form).HasColumnType("decimal(6,4)");
        builder.Property(x => x.WinRate).HasColumnType("decimal(6,4)");
        builder.Property(x => x.CommunityScore).HasColumnType("decimal(6,4)");
        builder.Property(x => x.Trust).HasColumnType("decimal(6,2)");
        builder.Property(x => x.Contribution).HasColumnType("decimal(6,4)");
        builder.Property(x => x.AssessmentsJson).HasColumnType("text");
        builder.Property(x => x.Summary).HasColumnType("text");
        builder.Property(x => x.UpdatedAt).HasColumnType("timestamp with time zone");

        builder.HasOne(x => x.Player)
            .WithMany()
            .HasForeignKey(x => x.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
