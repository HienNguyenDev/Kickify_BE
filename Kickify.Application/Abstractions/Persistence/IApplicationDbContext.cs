using Kickify.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Kickify.Application.Abstractions.Persistence;

public interface IApplicationDbContext
{
    // Identity Schema
    DbSet<User> Users { get; set; }
    DbSet<PlayerProfile> PlayerProfiles { get; set; }
    DbSet<NotificationPreference> NotificationPreferences { get; set; }
    DbSet<Achievement> Achievements { get; set; }
    DbSet<PlayerAchievement> PlayerAchievements { get; set; }
    DbSet<RefreshToken> RefreshTokens { get; set; }

    // Venue Schema
    DbSet<Venue> Venues { get; set; }
    DbSet<VenuePhoto> VenuePhotos { get; set; }
    DbSet<VenueOperatingHour> VenueOperatingHours { get; set; }
    DbSet<Field> Fields { get; set; }
    DbSet<VenueReview> VenueReviews { get; set; }

    // Match Schema
    DbSet<MatchRoom> MatchRooms { get; set; }
    DbSet<MatchPreset> MatchPresets { get; set; }
    DbSet<RoomParticipant> RoomParticipants { get; set; }
    DbSet<RoomInvitation> RoomInvitations { get; set; }
    DbSet<Booking> Bookings { get; set; }
    DbSet<ChatMessage> ChatMessages { get; set; }

    // Social Schema
    DbSet<Post> Posts { get; set; }
    DbSet<PostMedia> PostMedias { get; set; }
    DbSet<PostLike> PostLikes { get; set; }
    DbSet<Comment> Comments { get; set; }
    DbSet<CommentLike> CommentLikes { get; set; }
    DbSet<Friendship> Friendships { get; set; }
    DbSet<ContentReport> ContentReports { get; set; }

    // Evaluation Schema
    DbSet<MatchFeedback> MatchFeedbacks { get; set; }
    DbSet<EloHistory> EloHistories { get; set; }
    DbSet<EloConfiguration> EloConfigurations { get; set; }
    DbSet<PlayerReport> PlayerReports { get; set; }

    // System Schema
    DbSet<Notification> Notifications { get; set; }
    DbSet<Announcement> Announcements { get; set; }
    DbSet<SystemLog> SystemLogs { get; set; }

    // Payment Schema
    DbSet<PaymentRequest> PaymentRequests { get; set; }
    DbSet<Wallet> Wallets { get; set; }
    DbSet<WalletTransaction> WalletTransactions { get; set; }
    DbSet<WalletWithdrawal> WalletWithdrawals { get; set; }
}
