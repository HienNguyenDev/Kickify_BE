using Kickify.Domain.Entities;
using Kickify.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kickify.Infrastructure.Configurations;

public class RoomInvitationConfiguration : IEntityTypeConfiguration<RoomInvitation>
{
    public void Configure(EntityTypeBuilder<RoomInvitation> builder)
    {
        builder.ToTable("RoomInvitations", Schemas.Match);

        builder.HasKey(ri => ri.InvitationId);

        builder.Property(ri => ri.InvitationId)
            .IsRequired();

        builder.Property(ri => ri.RoomId)
            .IsRequired();

        builder.Property(ri => ri.InviterId)
            .IsRequired();

        builder.Property(ri => ri.InviteeId)
            .IsRequired();

        builder.Property(ri => ri.InvitationLink)
            .HasMaxLength(500);

        builder.Property(ri => ri.QrCodeUrl)
            .HasMaxLength(500);

        builder.Property(ri => ri.Status)
            .HasConversion<string>()
            .HasDefaultValue(Domain.Enums.InvitationStatus.Pending);

        builder.Property(ri => ri.RespondedAt)
            .HasColumnType("timestamp");

        // Indexes
        builder.HasIndex(ri => new { ri.RoomId, ri.InviteeId })
            .IsUnique();
    }
}
