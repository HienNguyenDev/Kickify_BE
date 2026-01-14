using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kickify.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "evaluation");

            migrationBuilder.EnsureSchema(
                name: "system");

            migrationBuilder.EnsureSchema(
                name: "venue");

            migrationBuilder.EnsureSchema(
                name: "social");

            migrationBuilder.EnsureSchema(
                name: "match");

            migrationBuilder.EnsureSchema(
                name: "identity");

            migrationBuilder.CreateTable(
                name: "Achievements",
                schema: "evaluation",
                columns: table => new
                {
                    AchievementId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    BadgeIconUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CriteriaType = table.Column<string>(type: "text", nullable: false),
                    CriteriaValue = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Achievements", x => x.AchievementId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                schema: "identity",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    FullName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    AvatarUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Bio = table.Column<string>(type: "text", nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "date", nullable: true),
                    Gender = table.Column<string>(type: "text", nullable: true),
                    Role = table.Column<string>(type: "text", nullable: false),
                    Positions = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true, comment: "JSON array: [\"ST\", \"CM\", \"CB\"]"),
                    ShirtNumber = table.Column<int>(type: "integer", nullable: true),
                    PreferredFoot = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    IdentityId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    IsEmailVerified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "Announcements",
                schema: "system",
                columns: table => new
                {
                    AnnouncementId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    AnnouncementType = table.Column<string>(type: "text", nullable: false),
                    Priority = table.Column<string>(type: "text", nullable: false),
                    ShowFrom = table.Column<DateTime>(type: "timestamp", nullable: false),
                    ShowTo = table.Column<DateTime>(type: "timestamp", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Announcements", x => x.AnnouncementId);
                    table.ForeignKey(
                        name: "FK_Announcements_Users_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EloConfigurations",
                schema: "evaluation",
                columns: table => new
                {
                    ConfigId = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    KBase = table.Column<decimal>(type: "numeric(5,2)", nullable: false, comment: "Base K factor"),
                    KWinloss = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    KPerformance = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    KFeedback = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    KSentiment = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    KTrust = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    KRole = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "date", nullable: false),
                    EffectiveTo = table.Column<DateTime>(type: "date", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EloConfigurations", x => x.ConfigId);
                    table.ForeignKey(
                        name: "FK_EloConfigurations_Users_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "NotificationPreferences",
                schema: "identity",
                columns: table => new
                {
                    PreferenceId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    MatchInvites = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    MatchResults = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    ChatMessages = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    RoomUpdates = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    SystemAnnouncements = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationPreferences", x => x.PreferenceId);
                    table.ForeignKey(
                        name: "FK_NotificationPreferences_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                schema: "identity",
                columns: table => new
                {
                    NotificationId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    NotificationType = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    RelatedEntityType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RelatedEntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ReadAt = table.Column<DateTime>(type: "timestamp", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.NotificationId);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlayerAchievements",
                schema: "evaluation",
                columns: table => new
                {
                    PlayerAchievementId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AchievementId = table.Column<Guid>(type: "uuid", nullable: false),
                    EarnedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerAchievements", x => x.PlayerAchievementId);
                    table.ForeignKey(
                        name: "FK_PlayerAchievements_Achievements_AchievementId",
                        column: x => x.AchievementId,
                        principalSchema: "evaluation",
                        principalTable: "Achievements",
                        principalColumn: "AchievementId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayerAchievements_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlayerProfiles",
                schema: "identity",
                columns: table => new
                {
                    ProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrentElo = table.Column<int>(type: "integer", nullable: false, defaultValue: 1000),
                    TrustScore = table.Column<decimal>(type: "numeric(5,2)", nullable: false, defaultValue: 100.00m),
                    TotalMatches = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Wins = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Losses = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Draws = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    MvpCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    WinStreak = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    MaxWinStreak = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    AfkCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ReportCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    PreferredPositions = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerProfiles", x => x.ProfileId);
                    table.ForeignKey(
                        name: "FK_PlayerProfiles_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Posts",
                schema: "social",
                columns: table => new
                {
                    PostId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    TotalMedia = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    TotalLikes = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    TotalComments = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Visibility = table.Column<string>(type: "text", nullable: false, defaultValue: "Public"),
                    IsEdited = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    EditedAt = table.Column<DateTime>(type: "timestamp", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Posts", x => x.PostId);
                    table.ForeignKey(
                        name: "FK_Posts_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                schema: "identity",
                columns: table => new
                {
                    TokenId = table.Column<Guid>(type: "uuid", nullable: false),
                    Token = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReplacedByToken = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.TokenId);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SystemLogs",
                schema: "system",
                columns: table => new
                {
                    LogId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Action = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    EntityType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    IpAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "text", nullable: true),
                    RequestDetails = table.Column<string>(type: "text", nullable: true, comment: "JSON"),
                    ResponseStatus = table.Column<int>(type: "integer", nullable: true),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemLogs", x => x.LogId);
                    table.ForeignKey(
                        name: "FK_SystemLogs_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Venues",
                schema: "venue",
                columns: table => new
                {
                    VenueId = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    VenueName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false),
                    Latitude = table.Column<decimal>(type: "numeric(10,8)", nullable: true),
                    Longitude = table.Column<decimal>(type: "numeric(11,8)", nullable: true),
                    ContactPhone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ContactEmail = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Amenities = table.Column<string>(type: "text", nullable: true, comment: "JSON: parking, shower, etc."),
                    Status = table.Column<string>(type: "text", nullable: false, defaultValue: "Pending"),
                    AdminNotes = table.Column<string>(type: "text", nullable: true),
                    AverageRating = table.Column<decimal>(type: "numeric(3,2)", nullable: false, defaultValue: 0m),
                    TotalReviews = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Venues", x => x.VenueId);
                    table.ForeignKey(
                        name: "FK_Venues_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Comments",
                schema: "social",
                columns: table => new
                {
                    CommentId = table.Column<Guid>(type: "uuid", nullable: false),
                    PostId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentCommentId = table.Column<Guid>(type: "uuid", nullable: true, comment: "NULL = root comment, NOT NULL = reply"),
                    Content = table.Column<string>(type: "text", nullable: false),
                    TotalLikes = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    TotalReplies = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Only count for root comments"),
                    IsEdited = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comments", x => x.CommentId);
                    table.ForeignKey(
                        name: "FK_Comments_Comments_ParentCommentId",
                        column: x => x.ParentCommentId,
                        principalSchema: "social",
                        principalTable: "Comments",
                        principalColumn: "CommentId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Comments_Posts_PostId",
                        column: x => x.PostId,
                        principalSchema: "social",
                        principalTable: "Posts",
                        principalColumn: "PostId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Comments_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PostLikes",
                schema: "social",
                columns: table => new
                {
                    LikeId = table.Column<Guid>(type: "uuid", nullable: false),
                    PostId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostLikes", x => x.LikeId);
                    table.ForeignKey(
                        name: "FK_PostLikes_Posts_PostId",
                        column: x => x.PostId,
                        principalSchema: "social",
                        principalTable: "Posts",
                        principalColumn: "PostId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PostLikes_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PostMedia",
                schema: "social",
                columns: table => new
                {
                    MediaId = table.Column<Guid>(type: "uuid", nullable: false),
                    PostId = table.Column<Guid>(type: "uuid", nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    StoragePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false, comment: "MinIO object path (e.g., images/2024/01/15/abc123.jpg)"),
                    PublicUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false, comment: "Full CDN URL for client access"),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "MIME type (e.g., image/jpeg, video/mp4)"),
                    BucketName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValue: "kickify-media", comment: "MinIO bucket name"),
                    MediaType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false, comment: "File size in bytes"),
                    ThumbnailStoragePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true, comment: "MinIO path for video thumbnail"),
                    ThumbnailUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true, comment: "Full CDN URL for video thumbnail"),
                    Duration = table.Column<int>(type: "integer", nullable: true, comment: "Video duration in seconds"),
                    Width = table.Column<int>(type: "integer", nullable: true, comment: "Media width in pixels"),
                    Height = table.Column<int>(type: "integer", nullable: true, comment: "Media height in pixels"),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IsProcessed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true, comment: "Processing status for async video encoding"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostMedia", x => x.MediaId);
                    table.ForeignKey(
                        name: "FK_PostMedia_Posts_PostId",
                        column: x => x.PostId,
                        principalSchema: "social",
                        principalTable: "Posts",
                        principalColumn: "PostId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Fields",
                schema: "venue",
                columns: table => new
                {
                    FieldId = table.Column<Guid>(type: "uuid", nullable: false),
                    VenueId = table.Column<Guid>(type: "uuid", nullable: false),
                    FieldName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FieldType = table.Column<string>(type: "text", nullable: false),
                    SurfaceType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "Grass, Artificial, etc."),
                    HourlyRate = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    PeakHourSurcharge = table.Column<decimal>(type: "numeric(10,2)", nullable: false, defaultValue: 0m),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fields", x => x.FieldId);
                    table.ForeignKey(
                        name: "FK_Fields_Venues_VenueId",
                        column: x => x.VenueId,
                        principalSchema: "venue",
                        principalTable: "Venues",
                        principalColumn: "VenueId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VenueOperatingHours",
                schema: "venue",
                columns: table => new
                {
                    HoursId = table.Column<Guid>(type: "uuid", nullable: false),
                    VenueId = table.Column<Guid>(type: "uuid", nullable: false),
                    DayOfWeek = table.Column<string>(type: "text", nullable: false),
                    OpenTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    CloseTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    IsClosed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VenueOperatingHours", x => x.HoursId);
                    table.ForeignKey(
                        name: "FK_VenueOperatingHours_Venues_VenueId",
                        column: x => x.VenueId,
                        principalSchema: "venue",
                        principalTable: "Venues",
                        principalColumn: "VenueId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VenuePhotos",
                schema: "venue",
                columns: table => new
                {
                    PhotoId = table.Column<Guid>(type: "uuid", nullable: false),
                    VenueId = table.Column<Guid>(type: "uuid", nullable: false),
                    PhotoUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VenuePhotos", x => x.PhotoId);
                    table.ForeignKey(
                        name: "FK_VenuePhotos_Venues_VenueId",
                        column: x => x.VenueId,
                        principalSchema: "venue",
                        principalTable: "Venues",
                        principalColumn: "VenueId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VenueWallets",
                schema: "venue",
                columns: table => new
                {
                    WalletId = table.Column<Guid>(type: "uuid", nullable: false),
                    VenueId = table.Column<Guid>(type: "uuid", nullable: false),
                    Balance = table.Column<decimal>(type: "numeric(12,2)", nullable: false, defaultValue: 0m),
                    BankAccountNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    BankName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AccountHolderName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsBankVerified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VenueWallets", x => x.WalletId);
                    table.ForeignKey(
                        name: "FK_VenueWallets_Venues_VenueId",
                        column: x => x.VenueId,
                        principalSchema: "venue",
                        principalTable: "Venues",
                        principalColumn: "VenueId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CommentLikes",
                schema: "social",
                columns: table => new
                {
                    LikeId = table.Column<Guid>(type: "uuid", nullable: false),
                    CommentId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommentLikes", x => x.LikeId);
                    table.ForeignKey(
                        name: "FK_CommentLikes_Comments_CommentId",
                        column: x => x.CommentId,
                        principalSchema: "social",
                        principalTable: "Comments",
                        principalColumn: "CommentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CommentLikes_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MatchPresets",
                schema: "match",
                columns: table => new
                {
                    PresetId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PresetName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FieldId = table.Column<Guid>(type: "uuid", nullable: true),
                    CustomLocation = table.Column<string>(type: "text", nullable: true),
                    MatchFormat = table.Column<string>(type: "text", nullable: false),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchPresets", x => x.PresetId);
                    table.ForeignKey(
                        name: "FK_MatchPresets_Fields_FieldId",
                        column: x => x.FieldId,
                        principalSchema: "venue",
                        principalTable: "Fields",
                        principalColumn: "FieldId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MatchPresets_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MatchRooms",
                schema: "match",
                columns: table => new
                {
                    RoomId = table.Column<Guid>(type: "uuid", nullable: false),
                    HostId = table.Column<Guid>(type: "uuid", nullable: false),
                    FieldId = table.Column<Guid>(type: "uuid", nullable: true),
                    CustomLocation = table.Column<string>(type: "text", nullable: true),
                    MatchFormat = table.Column<string>(type: "text", nullable: false),
                    MatchType = table.Column<string>(type: "text", nullable: false),
                    Visibility = table.Column<string>(type: "text", nullable: false, defaultValue: "Public"),
                    MatchDate = table.Column<DateTime>(type: "date", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Rules = table.Column<string>(type: "text", nullable: true),
                    TotalSlots = table.Column<int>(type: "integer", nullable: false),
                    FilledSlots = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    DepositPerPerson = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    TotalDepositCollected = table.Column<decimal>(type: "numeric(10,2)", nullable: false, defaultValue: 0m),
                    Status = table.Column<string>(type: "text", nullable: false, defaultValue: "Open"),
                    TeamAScore = table.Column<int>(type: "integer", nullable: true),
                    TeamBScore = table.Column<int>(type: "integer", nullable: true),
                    ResultConfirmedBy = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "count of confirmations"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchRooms", x => x.RoomId);
                    table.ForeignKey(
                        name: "FK_MatchRooms_Fields_FieldId",
                        column: x => x.FieldId,
                        principalSchema: "venue",
                        principalTable: "Fields",
                        principalColumn: "FieldId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MatchRooms_Users_HostId",
                        column: x => x.HostId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WalletTransactions",
                schema: "venue",
                columns: table => new
                {
                    TransactionId = table.Column<Guid>(type: "uuid", nullable: false),
                    WalletId = table.Column<Guid>(type: "uuid", nullable: false),
                    TransactionType = table.Column<string>(type: "text", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    BalanceAfter = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    ReferenceId = table.Column<Guid>(type: "uuid", nullable: true, comment: "booking_id or withdrawal_id"),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WalletTransactions", x => x.TransactionId);
                    table.ForeignKey(
                        name: "FK_WalletTransactions_VenueWallets_WalletId",
                        column: x => x.WalletId,
                        principalSchema: "venue",
                        principalTable: "VenueWallets",
                        principalColumn: "WalletId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Withdrawals",
                schema: "venue",
                columns: table => new
                {
                    WithdrawalId = table.Column<Guid>(type: "uuid", nullable: false),
                    WalletId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false, defaultValue: "Pending"),
                    RequestDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProcessedDate = table.Column<DateTime>(type: "timestamp", nullable: true),
                    AdminNotes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Withdrawals", x => x.WithdrawalId);
                    table.ForeignKey(
                        name: "FK_Withdrawals_VenueWallets_WalletId",
                        column: x => x.WalletId,
                        principalSchema: "venue",
                        principalTable: "VenueWallets",
                        principalColumn: "WalletId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Bookings",
                schema: "venue",
                columns: table => new
                {
                    BookingId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoomId = table.Column<Guid>(type: "uuid", nullable: false),
                    FieldId = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingDate = table.Column<DateTime>(type: "date", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    PlatformFee = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    VenueAmount = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false, defaultValue: "Confirmed"),
                    PaymentMethod = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TransactionReference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookings", x => x.BookingId);
                    table.ForeignKey(
                        name: "FK_Bookings_Fields_FieldId",
                        column: x => x.FieldId,
                        principalSchema: "venue",
                        principalTable: "Fields",
                        principalColumn: "FieldId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Bookings_MatchRooms_RoomId",
                        column: x => x.RoomId,
                        principalSchema: "match",
                        principalTable: "MatchRooms",
                        principalColumn: "RoomId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChatMessages",
                schema: "social",
                columns: table => new
                {
                    MessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoomId = table.Column<Guid>(type: "uuid", nullable: false),
                    SenderId = table.Column<Guid>(type: "uuid", nullable: false),
                    MessageText = table.Column<string>(type: "text", nullable: false),
                    MessageType = table.Column<string>(type: "text", nullable: false, defaultValue: "Text"),
                    IsEdited = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMessages", x => x.MessageId);
                    table.ForeignKey(
                        name: "FK_ChatMessages_MatchRooms_RoomId",
                        column: x => x.RoomId,
                        principalSchema: "match",
                        principalTable: "MatchRooms",
                        principalColumn: "RoomId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChatMessages_Users_SenderId",
                        column: x => x.SenderId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EloHistories",
                schema: "evaluation",
                columns: table => new
                {
                    EloHistoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    MatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    EloBefore = table.Column<int>(type: "integer", nullable: false),
                    EloAfter = table.Column<int>(type: "integer", nullable: false),
                    EloChange = table.Column<int>(type: "integer", nullable: false),
                    WinLossComponent = table.Column<decimal>(type: "numeric(6,2)", nullable: true),
                    PerformanceComponent = table.Column<decimal>(type: "numeric(6,2)", nullable: true),
                    FeedbackComponent = table.Column<decimal>(type: "numeric(6,2)", nullable: true),
                    SentimentComponent = table.Column<decimal>(type: "numeric(6,2)", nullable: true),
                    TrustComponent = table.Column<decimal>(type: "numeric(6,2)", nullable: true),
                    RoleComponent = table.Column<decimal>(type: "numeric(6,2)", nullable: true),
                    CalculationDetails = table.Column<string>(type: "text", nullable: true, comment: "JSON with all coefficients"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EloHistories", x => x.EloHistoryId);
                    table.ForeignKey(
                        name: "FK_EloHistories_MatchRooms_MatchId",
                        column: x => x.MatchId,
                        principalSchema: "match",
                        principalTable: "MatchRooms",
                        principalColumn: "RoomId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EloHistories_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MatchFeedbacks",
                schema: "evaluation",
                columns: table => new
                {
                    FeedbackId = table.Column<Guid>(type: "uuid", nullable: false),
                    MatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReviewerId = table.Column<Guid>(type: "uuid", nullable: false),
                    RevieweeId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamworkRating = table.Column<int>(type: "integer", nullable: false, comment: "1-5"),
                    FairplayRating = table.Column<int>(type: "integer", nullable: false, comment: "1-5"),
                    AttackRating = table.Column<int>(type: "integer", nullable: false, comment: "1-5"),
                    DefenseRating = table.Column<int>(type: "integer", nullable: false, comment: "1-5"),
                    CommunicationRating = table.Column<int>(type: "integer", nullable: false, comment: "1-5"),
                    AverageRating = table.Column<decimal>(type: "numeric(3,2)", nullable: false),
                    Comment = table.Column<string>(type: "text", nullable: true),
                    SentimentScore = table.Column<decimal>(type: "numeric(5,2)", nullable: true, comment: "AI-analyzed: -1.00 to 1.00"),
                    SentimentLabel = table.Column<string>(type: "text", nullable: true),
                    RevieweeResponse = table.Column<string>(type: "text", nullable: true),
                    ResponseDate = table.Column<DateTime>(type: "timestamp", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchFeedbacks", x => x.FeedbackId);
                    table.ForeignKey(
                        name: "FK_MatchFeedbacks_MatchRooms_MatchId",
                        column: x => x.MatchId,
                        principalSchema: "match",
                        principalTable: "MatchRooms",
                        principalColumn: "RoomId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MatchFeedbacks_Users_RevieweeId",
                        column: x => x.RevieweeId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MatchFeedbacks_Users_ReviewerId",
                        column: x => x.ReviewerId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlayerReports",
                schema: "evaluation",
                columns: table => new
                {
                    ReportId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReporterId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportedId = table.Column<Guid>(type: "uuid", nullable: false),
                    MatchId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReportType = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    EvidenceUrls = table.Column<string>(type: "text", nullable: true, comment: "JSON array of screenshot URLs"),
                    Status = table.Column<string>(type: "text", nullable: false, defaultValue: "Pending"),
                    AdminNotes = table.Column<string>(type: "text", nullable: true),
                    ActionTaken = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ResolvedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerReports", x => x.ReportId);
                    table.ForeignKey(
                        name: "FK_PlayerReports_MatchRooms_MatchId",
                        column: x => x.MatchId,
                        principalSchema: "match",
                        principalTable: "MatchRooms",
                        principalColumn: "RoomId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlayerReports_Users_ReportedId",
                        column: x => x.ReportedId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlayerReports_Users_ReporterId",
                        column: x => x.ReporterId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlayerReports_Users_ResolvedBy",
                        column: x => x.ResolvedBy,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RoomInvitations",
                schema: "match",
                columns: table => new
                {
                    InvitationId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoomId = table.Column<Guid>(type: "uuid", nullable: false),
                    InviterId = table.Column<Guid>(type: "uuid", nullable: false),
                    InviteeId = table.Column<Guid>(type: "uuid", nullable: false),
                    InvitationLink = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    QrCodeUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false, defaultValue: "Pending"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RespondedAt = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomInvitations", x => x.InvitationId);
                    table.ForeignKey(
                        name: "FK_RoomInvitations_MatchRooms_RoomId",
                        column: x => x.RoomId,
                        principalSchema: "match",
                        principalTable: "MatchRooms",
                        principalColumn: "RoomId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoomInvitations_Users_InviteeId",
                        column: x => x.InviteeId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RoomInvitations_Users_InviterId",
                        column: x => x.InviterId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RoomParticipants",
                schema: "match",
                columns: table => new
                {
                    ParticipantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoomId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamAssignment = table.Column<string>(type: "text", nullable: false),
                    Position = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true, comment: "ST, CM, CB, etc."),
                    JoinDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DepositPaid = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DepositAmount = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    CheckedIn = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CheckInTime = table.Column<DateTime>(type: "timestamp", nullable: true),
                    RemovalReason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomParticipants", x => x.ParticipantId);
                    table.ForeignKey(
                        name: "FK_RoomParticipants_MatchRooms_RoomId",
                        column: x => x.RoomId,
                        principalSchema: "match",
                        principalTable: "MatchRooms",
                        principalColumn: "RoomId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoomParticipants_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VenueReviews",
                schema: "venue",
                columns: table => new
                {
                    ReviewId = table.Column<Guid>(type: "uuid", nullable: false),
                    VenueId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingId = table.Column<Guid>(type: "uuid", nullable: false),
                    Rating = table.Column<int>(type: "integer", nullable: false, comment: "1-5"),
                    Comment = table.Column<string>(type: "text", nullable: true),
                    OwnerResponse = table.Column<string>(type: "text", nullable: true),
                    ResponseDate = table.Column<DateTime>(type: "timestamp", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VenueReviews", x => x.ReviewId);
                    table.ForeignKey(
                        name: "FK_VenueReviews_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalSchema: "venue",
                        principalTable: "Bookings",
                        principalColumn: "BookingId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VenueReviews_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VenueReviews_Venues_VenueId",
                        column: x => x.VenueId,
                        principalSchema: "venue",
                        principalTable: "Venues",
                        principalColumn: "VenueId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Announcements_CreatorUserId",
                schema: "system",
                table: "Announcements",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Announcements_IsActive",
                schema: "system",
                table: "Announcements",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Announcements_ShowFrom",
                schema: "system",
                table: "Announcements",
                column: "ShowFrom");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_BookingDate",
                schema: "venue",
                table: "Bookings",
                column: "BookingDate");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_FieldId",
                schema: "venue",
                table: "Bookings",
                column: "FieldId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_RoomId",
                schema: "venue",
                table: "Bookings",
                column: "RoomId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_RoomId",
                schema: "social",
                table: "ChatMessages",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_SenderId",
                schema: "social",
                table: "ChatMessages",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_SentAt",
                schema: "social",
                table: "ChatMessages",
                column: "SentAt");

            migrationBuilder.CreateIndex(
                name: "IX_CommentLikes_CommentId",
                schema: "social",
                table: "CommentLikes",
                column: "CommentId");

            migrationBuilder.CreateIndex(
                name: "IX_CommentLikes_CommentId_UserId",
                schema: "social",
                table: "CommentLikes",
                columns: new[] { "CommentId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CommentLikes_UserId",
                schema: "social",
                table: "CommentLikes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_CreatedAt",
                schema: "social",
                table: "Comments",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_ParentCommentId",
                schema: "social",
                table: "Comments",
                column: "ParentCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_PostId",
                schema: "social",
                table: "Comments",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_UserId",
                schema: "social",
                table: "Comments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_EloConfigurations_CreatorUserId",
                schema: "evaluation",
                table: "EloConfigurations",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EloConfigurations_EffectiveFrom",
                schema: "evaluation",
                table: "EloConfigurations",
                column: "EffectiveFrom");

            migrationBuilder.CreateIndex(
                name: "IX_EloConfigurations_IsActive",
                schema: "evaluation",
                table: "EloConfigurations",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_EloHistories_CreatedAt",
                schema: "evaluation",
                table: "EloHistories",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_EloHistories_MatchId",
                schema: "evaluation",
                table: "EloHistories",
                column: "MatchId");

            migrationBuilder.CreateIndex(
                name: "IX_EloHistories_UserId",
                schema: "evaluation",
                table: "EloHistories",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Fields_VenueId",
                schema: "venue",
                table: "Fields",
                column: "VenueId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchFeedbacks_MatchId",
                schema: "evaluation",
                table: "MatchFeedbacks",
                column: "MatchId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchFeedbacks_MatchId_ReviewerId_RevieweeId",
                schema: "evaluation",
                table: "MatchFeedbacks",
                columns: new[] { "MatchId", "ReviewerId", "RevieweeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MatchFeedbacks_RevieweeId",
                schema: "evaluation",
                table: "MatchFeedbacks",
                column: "RevieweeId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchFeedbacks_ReviewerId",
                schema: "evaluation",
                table: "MatchFeedbacks",
                column: "ReviewerId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchPresets_FieldId",
                schema: "match",
                table: "MatchPresets",
                column: "FieldId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchPresets_UserId",
                schema: "match",
                table: "MatchPresets",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchRooms_FieldId",
                schema: "match",
                table: "MatchRooms",
                column: "FieldId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchRooms_HostId",
                schema: "match",
                table: "MatchRooms",
                column: "HostId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchRooms_MatchDate",
                schema: "match",
                table: "MatchRooms",
                column: "MatchDate");

            migrationBuilder.CreateIndex(
                name: "IX_MatchRooms_Status",
                schema: "match",
                table: "MatchRooms",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationPreferences_UserId",
                schema: "identity",
                table: "NotificationPreferences",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CreatedAt",
                schema: "identity",
                table: "Notifications",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_IsRead",
                schema: "identity",
                table: "Notifications",
                column: "IsRead");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                schema: "identity",
                table: "Notifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerAchievements_AchievementId",
                schema: "evaluation",
                table: "PlayerAchievements",
                column: "AchievementId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerAchievements_UserId_AchievementId",
                schema: "evaluation",
                table: "PlayerAchievements",
                columns: new[] { "UserId", "AchievementId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlayerProfiles_UserId",
                schema: "identity",
                table: "PlayerProfiles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlayerReports_MatchId",
                schema: "evaluation",
                table: "PlayerReports",
                column: "MatchId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerReports_ReportedId",
                schema: "evaluation",
                table: "PlayerReports",
                column: "ReportedId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerReports_ReporterId",
                schema: "evaluation",
                table: "PlayerReports",
                column: "ReporterId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerReports_ResolvedBy",
                schema: "evaluation",
                table: "PlayerReports",
                column: "ResolvedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerReports_Status",
                schema: "evaluation",
                table: "PlayerReports",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PostLikes_PostId",
                schema: "social",
                table: "PostLikes",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_PostLikes_PostId_UserId",
                schema: "social",
                table: "PostLikes",
                columns: new[] { "PostId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PostLikes_UserId",
                schema: "social",
                table: "PostLikes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PostMedia_IsProcessed",
                schema: "social",
                table: "PostMedia",
                column: "IsProcessed");

            migrationBuilder.CreateIndex(
                name: "IX_PostMedia_MediaType",
                schema: "social",
                table: "PostMedia",
                column: "MediaType");

            migrationBuilder.CreateIndex(
                name: "IX_PostMedia_PostId",
                schema: "social",
                table: "PostMedia",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_PostMedia_PostId_DisplayOrder",
                schema: "social",
                table: "PostMedia",
                columns: new[] { "PostId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_PostMedia_StoragePath",
                schema: "social",
                table: "PostMedia",
                column: "StoragePath");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_CreatedAt",
                schema: "social",
                table: "Posts",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_IsActive",
                schema: "social",
                table: "Posts",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_UserId",
                schema: "social",
                table: "Posts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_Token",
                schema: "identity",
                table: "RefreshTokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                schema: "identity",
                table: "RefreshTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RoomInvitations_InviteeId",
                schema: "match",
                table: "RoomInvitations",
                column: "InviteeId");

            migrationBuilder.CreateIndex(
                name: "IX_RoomInvitations_InviterId",
                schema: "match",
                table: "RoomInvitations",
                column: "InviterId");

            migrationBuilder.CreateIndex(
                name: "IX_RoomInvitations_RoomId_InviteeId",
                schema: "match",
                table: "RoomInvitations",
                columns: new[] { "RoomId", "InviteeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoomParticipants_RoomId",
                schema: "match",
                table: "RoomParticipants",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_RoomParticipants_RoomId_UserId",
                schema: "match",
                table: "RoomParticipants",
                columns: new[] { "RoomId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoomParticipants_UserId",
                schema: "match",
                table: "RoomParticipants",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SystemLogs_Action",
                schema: "system",
                table: "SystemLogs",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_SystemLogs_CreatedAt",
                schema: "system",
                table: "SystemLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SystemLogs_EntityType",
                schema: "system",
                table: "SystemLogs",
                column: "EntityType");

            migrationBuilder.CreateIndex(
                name: "IX_SystemLogs_UserId",
                schema: "system",
                table: "SystemLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                schema: "identity",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_IdentityId",
                schema: "identity",
                table: "Users",
                column: "IdentityId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VenueOperatingHours_VenueId_DayOfWeek",
                schema: "venue",
                table: "VenueOperatingHours",
                columns: new[] { "VenueId", "DayOfWeek" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VenuePhotos_VenueId",
                schema: "venue",
                table: "VenuePhotos",
                column: "VenueId");

            migrationBuilder.CreateIndex(
                name: "IX_VenueReviews_BookingId",
                schema: "venue",
                table: "VenueReviews",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_VenueReviews_UserId_BookingId",
                schema: "venue",
                table: "VenueReviews",
                columns: new[] { "UserId", "BookingId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VenueReviews_VenueId",
                schema: "venue",
                table: "VenueReviews",
                column: "VenueId");

            migrationBuilder.CreateIndex(
                name: "IX_Venues_OwnerId",
                schema: "venue",
                table: "Venues",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Venues_Status",
                schema: "venue",
                table: "Venues",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_VenueWallets_VenueId",
                schema: "venue",
                table: "VenueWallets",
                column: "VenueId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_CreatedAt",
                schema: "venue",
                table: "WalletTransactions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_WalletId",
                schema: "venue",
                table: "WalletTransactions",
                column: "WalletId");

            migrationBuilder.CreateIndex(
                name: "IX_Withdrawals_WalletId",
                schema: "venue",
                table: "Withdrawals",
                column: "WalletId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Announcements",
                schema: "system");

            migrationBuilder.DropTable(
                name: "ChatMessages",
                schema: "social");

            migrationBuilder.DropTable(
                name: "CommentLikes",
                schema: "social");

            migrationBuilder.DropTable(
                name: "EloConfigurations",
                schema: "evaluation");

            migrationBuilder.DropTable(
                name: "EloHistories",
                schema: "evaluation");

            migrationBuilder.DropTable(
                name: "MatchFeedbacks",
                schema: "evaluation");

            migrationBuilder.DropTable(
                name: "MatchPresets",
                schema: "match");

            migrationBuilder.DropTable(
                name: "NotificationPreferences",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "Notifications",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "PlayerAchievements",
                schema: "evaluation");

            migrationBuilder.DropTable(
                name: "PlayerProfiles",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "PlayerReports",
                schema: "evaluation");

            migrationBuilder.DropTable(
                name: "PostLikes",
                schema: "social");

            migrationBuilder.DropTable(
                name: "PostMedia",
                schema: "social");

            migrationBuilder.DropTable(
                name: "RefreshTokens",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "RoomInvitations",
                schema: "match");

            migrationBuilder.DropTable(
                name: "RoomParticipants",
                schema: "match");

            migrationBuilder.DropTable(
                name: "SystemLogs",
                schema: "system");

            migrationBuilder.DropTable(
                name: "VenueOperatingHours",
                schema: "venue");

            migrationBuilder.DropTable(
                name: "VenuePhotos",
                schema: "venue");

            migrationBuilder.DropTable(
                name: "VenueReviews",
                schema: "venue");

            migrationBuilder.DropTable(
                name: "WalletTransactions",
                schema: "venue");

            migrationBuilder.DropTable(
                name: "Withdrawals",
                schema: "venue");

            migrationBuilder.DropTable(
                name: "Comments",
                schema: "social");

            migrationBuilder.DropTable(
                name: "Achievements",
                schema: "evaluation");

            migrationBuilder.DropTable(
                name: "Bookings",
                schema: "venue");

            migrationBuilder.DropTable(
                name: "VenueWallets",
                schema: "venue");

            migrationBuilder.DropTable(
                name: "Posts",
                schema: "social");

            migrationBuilder.DropTable(
                name: "MatchRooms",
                schema: "match");

            migrationBuilder.DropTable(
                name: "Fields",
                schema: "venue");

            migrationBuilder.DropTable(
                name: "Venues",
                schema: "venue");

            migrationBuilder.DropTable(
                name: "Users",
                schema: "identity");
        }
    }
}
