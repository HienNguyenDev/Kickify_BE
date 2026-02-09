using Kickify.Domain.Entities;
using Kickify.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kickify.Infrastructure.Configurations;

public class FormationAssignmentConfiguration : IEntityTypeConfiguration<FormationAssignment>
{
    public void Configure(EntityTypeBuilder<FormationAssignment> builder)
    {
        builder.ToTable("FormationAssignments", Schemas.Match);

        builder.HasKey(fa => fa.AssignmentId);

        builder.Property(fa => fa.AssignmentId)
            .IsRequired();

        builder.Property(fa => fa.FormationId)
            .IsRequired();

        builder.Property(fa => fa.PlayerId)
            .IsRequired();

        builder.Property(fa => fa.SlotId)
            .HasMaxLength(10)
            .IsRequired();

        // Relationship with MatchFormation
        builder.HasOne(fa => fa.Formation)
            .WithMany(mf => mf.Assignments)
            .HasForeignKey(fa => fa.FormationId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relationship with User (Player)
        builder.HasOne(fa => fa.Player)
            .WithMany()
            .HasForeignKey(fa => fa.PlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Unique constraints
        // Each slot can only have one player per formation
        builder.HasIndex(fa => new { fa.FormationId, fa.SlotId })
            .IsUnique();

        // Each player can only be in one slot per formation
        builder.HasIndex(fa => new { fa.FormationId, fa.PlayerId })
            .IsUnique();
    }
}
