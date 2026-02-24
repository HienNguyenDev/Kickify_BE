using Kickify.Domain.Entities;
using Kickify.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kickify.Infrastructure.Configurations;

public class MatchFormationConfiguration : IEntityTypeConfiguration<MatchFormation>
{
    public void Configure(EntityTypeBuilder<MatchFormation> builder)
    {
        builder.ToTable("MatchFormations", Schemas.Match);

        builder.HasKey(mf => mf.FormationId);

        builder.Property(mf => mf.FormationId)
            .IsRequired();

        builder.Property(mf => mf.RoomId)
            .IsRequired();

        builder.Property(mf => mf.TeamAssignment)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(mf => mf.FormationName)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(mf => mf.MatchFormat)
            .HasMaxLength(50)
            .IsRequired();

        // Relationship with MatchRoom
        builder.HasOne(mf => mf.MatchRoom)
            .WithMany(mr => mr.Formations)
            .HasForeignKey(mf => mf.RoomId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique constraint: one formation per team per room
        builder.HasIndex(mf => new { mf.RoomId, mf.TeamAssignment })
            .IsUnique();
    }
}
