using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Infrastructure.Database;
using Kickify.Infrastructure.Persistence.Converter;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Kickify.Infrastructure.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users", Schemas.Identity);

        builder.HasKey(u => u.UserId);

        builder.Property(u => u.UserId)
            .IsRequired();

        builder.Property(u => u.Email)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(u => u.PasswordHash)
            .HasMaxLength(255);

        builder.Property(u => u.FullName)
            .HasMaxLength(100);

        builder.Property(u => u.Phone)
            .HasMaxLength(20);

        builder.Property(u => u.AvatarUrl)
            .HasMaxLength(500);

        builder.Property(u => u.Bio)
            .HasColumnType("text");

        builder.Property(u => u.DateOfBirth)
            .HasColumnType("date");

        builder.Property(u => u.Gender)
            .HasConversion<string>();

        builder.Property(u => u.Role)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(p => p.Positions)
            .HasMaxLength(255)
            .HasComment("JSON array: [\"ST\", \"CM\", \"CB\"]");

        builder.Property(u => u.ShirtNumber);

        builder.Property(u => u.PreferredFoot)
            .HasMaxLength(20);

        builder.Property(u => u.IdentityId)
            .HasMaxLength(255);

        builder.Property(u => u.IsEmailVerified)
            .HasDefaultValue(false);

        builder.Property(u => u.IsActive)
            .HasDefaultValue(true);

        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasFilter("\"DeletedAt\" IS NULL");

        builder.HasIndex(u => u.IdentityId)
            .IsUnique();

        builder.HasOne(u => u.PlayerProfile)
            .WithOne(p => p.User)
            .HasForeignKey<PlayerProfile>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(u => u.NotificationPreference)
            .WithOne(n => n.User)
            .HasForeignKey<NotificationPreference>(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.PlayerAchievements)
            .WithOne(pa => pa.User)
            .HasForeignKey(pa => pa.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.Venues)
            .WithOne(v => v.Owner)
            .HasForeignKey(v => v.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.VenueReviews)
            .WithOne(vr => vr.User)
            .HasForeignKey(vr => vr.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.HostedRooms)
            .WithOne(mr => mr.Host)
            .HasForeignKey(mr => mr.HostId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.MatchPresets)
            .WithOne(mp => mp.User)
            .HasForeignKey(mp => mp.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.RoomParticipations)
            .WithOne(rp => rp.User)
            .HasForeignKey(rp => rp.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.SentInvitations)
            .WithOne(ri => ri.Inviter)
            .HasForeignKey(ri => ri.InviterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.ReceivedInvitations)
            .WithOne(ri => ri.Invitee)
            .HasForeignKey(ri => ri.InviteeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.ChatMessages)
            .WithOne(cm => cm.Sender)
            .HasForeignKey(cm => cm.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.GivenFeedbacks)
            .WithOne(mf => mf.Reviewer)
            .HasForeignKey(mf => mf.ReviewerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.ReceivedFeedbacks)
            .WithOne(mf => mf.Reviewee)
            .HasForeignKey(mf => mf.RevieweeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.EloHistories)
            .WithOne(eh => eh.User)
            .HasForeignKey(eh => eh.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.ReportsMade)
            .WithOne(pr => pr.Reporter)
            .HasForeignKey(pr => pr.ReporterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.ReportsReceived)
            .WithOne(pr => pr.Reported)
            .HasForeignKey(pr => pr.ReportedId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.ReportsResolved)
            .WithOne(pr => pr.Resolver)
            .HasForeignKey(pr => pr.ResolvedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.SystemLogs)
            .WithOne(sl => sl.User)
            .HasForeignKey(sl => sl.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(u => u.Notifications)
            .WithOne(n => n.User)
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.Posts)
            .WithOne(p => p.User)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.PostLikes)
            .WithOne(pl => pl.User)
            .HasForeignKey(pl => pl.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.Comments)
            .WithOne(c => c.User)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.CommentLikes)
            .WithOne(cl => cl.User)
            .HasForeignKey(cl => cl.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.RefreshTokens)
            .WithOne(rt => rt.User)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
