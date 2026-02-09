using Kickify.Domain.Entities;
using Kickify.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kickify.Infrastructure.Configurations;

public class MatchRoomConfiguration : IEntityTypeConfiguration<MatchRoom>
{
    public void Configure(EntityTypeBuilder<MatchRoom> builder)
    {
        builder.ToTable("MatchRooms", Schemas.Match);

        builder.HasKey(mr => mr.RoomId);

        builder.Property(mr => mr.RoomId)
            .IsRequired();

        builder.Property(mr => mr.HostId)
            .IsRequired();

        builder.Property(mr => mr.FieldId);

        builder.Property(mr => mr.CustomLocation)
            .HasColumnType("text");

        builder.Property(mr => mr.MatchFormat)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(mr => mr.MatchType)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(mr => mr.Visibility)
            .HasConversion<string>()
            .HasDefaultValue(Domain.Enums.Visibility.Public);

        builder.Property(mr => mr.MatchDate)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(mr => mr.StartTime)
            .HasColumnType("time")
            .IsRequired();

        builder.Property(mr => mr.DurationMinutes)
            .IsRequired();

        builder.Property(mr => mr.Description)
            .HasColumnType("text");

        builder.Property(mr => mr.Rules)
            .HasColumnType("text");

        builder.Property(mr => mr.TotalSlots)
            .IsRequired();

        builder.Property(mr => mr.FilledSlots)
            .HasDefaultValue(0);

        builder.Property(mr => mr.DepositPerPerson)
            .HasColumnType("decimal(10,2)");

        builder.Property(mr => mr.TotalDepositCollected)
            .HasColumnType("decimal(10,2)")
            .HasDefaultValue(0);

        builder.Property(mr => mr.Status)
            .HasConversion<string>()
            .HasDefaultValue(Domain.Enums.RoomStatus.Open);

        builder.Property(mr => mr.TeamAScore);

        builder.Property(mr => mr.TeamBScore);

        builder.Property(mr => mr.ResultConfirmedBy)
            .HasDefaultValue(0)
            .HasComment("count of confirmations");

        builder.Property(mr => mr.AutoCloseJobId)
            .HasMaxLength(100);

        // Indexes
        builder.HasIndex(mr => mr.HostId);
        builder.HasIndex(mr => mr.FieldId);
        builder.HasIndex(mr => mr.Status);
        builder.HasIndex(mr => mr.MatchDate);

        // Relationships
        builder.HasMany(mr => mr.RoomParticipants)
            .WithOne(rp => rp.MatchRoom)
            .HasForeignKey(rp => rp.RoomId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(mr => mr.RoomInvitations)
            .WithOne(ri => ri.MatchRoom)
            .HasForeignKey(ri => ri.RoomId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(mr => mr.Booking)
            .WithOne(b => b.MatchRoom)
            .HasForeignKey<Booking>(b => b.RoomId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(mr => mr.ChatMessages)
            .WithOne(cm => cm.MatchRoom)
            .HasForeignKey(cm => cm.RoomId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(mr => mr.MatchFeedbacks)
            .WithOne(mf => mf.Match)
            .HasForeignKey(mf => mf.MatchId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(mr => mr.EloHistories)
            .WithOne(eh => eh.Match)
            .HasForeignKey(eh => eh.MatchId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(mr => mr.PlayerReports)
            .WithOne(pr => pr.Match)
            .HasForeignKey(pr => pr.MatchId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
