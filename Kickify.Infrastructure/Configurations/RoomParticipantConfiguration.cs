using Kickify.Domain.Entities;
using Kickify.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kickify.Infrastructure.Configurations;

public class RoomParticipantConfiguration : IEntityTypeConfiguration<RoomParticipant>
{
    public void Configure(EntityTypeBuilder<RoomParticipant> builder)
    {
        builder.ToTable("RoomParticipants", Schemas.Match);

        builder.HasKey(rp => rp.ParticipantId);

        builder.Property(rp => rp.ParticipantId)
            .IsRequired();

        builder.Property(rp => rp.RoomId)
            .IsRequired();

        builder.Property(rp => rp.UserId)
            .IsRequired();

        builder.Property(rp => rp.TeamAssignment)
            .HasConversion<string>();

        builder.Property(rp => rp.Position)
            .HasMaxLength(10)
            .HasComment("ST, CM, CB, etc.");

        builder.Property(rp => rp.JoinDate)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(rp => rp.DepositPaid)
            .HasDefaultValue(false);

        builder.Property(rp => rp.DepositAmount)
            .HasColumnType("decimal(10,2)");

        builder.Property(rp => rp.CheckedIn)
            .HasDefaultValue(false);

        builder.Property(rp => rp.CheckInTime)
            .HasColumnType("timestamp");

        builder.Property(rp => rp.RemovalReason)
            .HasColumnType("text");

        builder.Property(rp => rp.AfkVoteCount)
            .HasDefaultValue(0);

        builder.Property(rp => rp.IsConfirmedAfk)
            .HasDefaultValue(false);

        // Indexes
        builder.HasIndex(rp => new { rp.RoomId, rp.UserId })
            .IsUnique();

        builder.HasIndex(rp => rp.RoomId);
    }
}
