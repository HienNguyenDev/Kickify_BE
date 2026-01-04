using Kickify.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kickify.Application.Abstractions.Persistence
{
    public interface IApplicationDbContext
    {
        DbSet<User> Users { get; set; }
        DbSet<PlayerProfile> PlayerProfiles { get; set; }
        DbSet<NotificationPreference> NotificationPreferences { get; set; }
        DbSet<Achievement> Achievements { get; set; }
        DbSet<PlayerAchievement> PlayerAchievements { get; set; }
        DbSet<Venue> Venues { get; set; }
        DbSet<VenuePhoto> VenuePhotos { get; set; }
        DbSet<VenueOperatingHour> VenueOperatingHours { get; set; }
        DbSet<Field> Fields { get; set; }
        DbSet<VenueWallet> VenueWallets { get; set; }
        DbSet<WalletTransaction> WalletTransactions { get; set; }
        DbSet<Withdrawal> Withdrawals { get; set; }
        DbSet<VenueReview> VenueReviews { get; set; }

        // Match Schema
        DbSet<MatchRoom> MatchRooms { get; set; }
        DbSet<MatchPreset> MatchPresets { get; set; }
        DbSet<RoomParticipant> RoomParticipants { get; set; }
        DbSet<RoomInvitation> RoomInvitations { get; set; }
        DbSet<Booking> Bookings { get; set; }
        DbSet<ChatMessage> ChatMessages { get; set; }
        DbSet<Post> Posts { get; set; }
        DbSet<PostMedia> PostMedias { get; set; }
        DbSet<PostLike> PostLikes { get; set; }
        DbSet<Comment> Comments { get; set; }
        DbSet<CommentLike> CommentLikes { get; set; }

        // Evaluation Schema
        DbSet<MatchFeedback> MatchFeedbacks { get; set; }
        DbSet<EloHistory> EloHistories { get; set; }
        DbSet<EloConfiguration> EloConfigurations { get; set; }
        DbSet<PlayerReport> PlayerReports { get; set; }

        // System Schema
        DbSet<Notification> Notifications { get; set; }
        DbSet<Announcement> Announcements { get; set; }
        DbSet<SystemLog> SystemLogs { get; set; }
    }
}
