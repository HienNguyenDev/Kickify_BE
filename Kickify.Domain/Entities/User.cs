using Kickify.Domain.Common;
using Kickify.Domain.Enums;

namespace Kickify.Domain.Entities;

public class User : Entity
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? PasswordHash { get; set; }
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public Gender? Gender { get; set; }
    public UserRole Role { get; set; }
    public string? PreferredPositions { get; set; }
    public int? ShirtNumber { get; set; }
    public string? PreferredFoot { get; set; }
    public string? IdentityId { get; set; }
    public bool IsEmailVerified { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public DateTime? BannedUntil { get; set; }
    public string? FcmToken { get; set; }

    // Navigation properties
    public PlayerProfile? PlayerProfile { get; set; }
    public Wallet? Wallet { get; set; }
    public NotificationPreference? NotificationPreference { get; set; }
    public ICollection<PlayerAchievement> PlayerAchievements { get; set; } = new List<PlayerAchievement>();
    public ICollection<Venue> Venues { get; set; } = new List<Venue>();
    public ICollection<VenueReview> VenueReviews { get; set; } = new List<VenueReview>();
    public ICollection<MatchRoom> HostedRooms { get; set; } = new List<MatchRoom>();
    public ICollection<MatchPreset> MatchPresets { get; set; } = new List<MatchPreset>();
    public ICollection<RoomParticipant> RoomParticipations { get; set; } = new List<RoomParticipant>();
    public ICollection<RoomInvitation> SentInvitations { get; set; } = new List<RoomInvitation>();
    public ICollection<RoomInvitation> ReceivedInvitations { get; set; } = new List<RoomInvitation>();
    public ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();
    public ICollection<MatchFeedback> GivenFeedbacks { get; set; } = new List<MatchFeedback>();
    public ICollection<MatchFeedback> ReceivedFeedbacks { get; set; } = new List<MatchFeedback>();
    public ICollection<EloHistory> EloHistories { get; set; } = new List<EloHistory>();
    public ICollection<PlayerReport> ReportsMade { get; set; } = new List<PlayerReport>();
    public ICollection<PlayerReport> ReportsReceived { get; set; } = new List<PlayerReport>();
    public ICollection<PlayerReport> ReportsResolved { get; set; } = new List<PlayerReport>();
    public ICollection<Announcement> Announcements { get; set; } = new List<Announcement>();
    public ICollection<SystemLog> SystemLogs { get; set; } = new List<SystemLog>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<Post> Posts { get; set; } = new List<Post>();
    public ICollection<PostLike> PostLikes { get; set; } = new List<PostLike>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<CommentLike> CommentLikes { get; set; } = new List<CommentLike>();
    public ICollection<EloConfiguration> EloConfigurations { get; set; } = new List<EloConfiguration>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
