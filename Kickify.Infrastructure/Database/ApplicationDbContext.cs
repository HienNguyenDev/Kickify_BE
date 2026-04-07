using Kickify.Application.Abstractions.Persistence;
using Kickify.Domain.Common;
using Kickify.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Kickify.Infrastructure.Database;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IPublisher publisher) : DbContext(options), IApplicationDbContext
{
    // Identity Schema
    public DbSet<User> Users { get; set; }
    public DbSet<PlayerProfile> PlayerProfiles { get; set; }
    public DbSet<NotificationPreference> NotificationPreferences { get; set; }
    public DbSet<Achievement> Achievements { get; set; }
    public DbSet<PlayerAchievement> PlayerAchievements { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    // Venue Schema
    public DbSet<Venue> Venues { get; set; }
    public DbSet<Holiday> Holidays { get; set; }
    public DbSet<VenuePhoto> VenuePhotos { get; set; }
    public DbSet<VenueOperatingHour> VenueOperatingHours { get; set; }
    public DbSet<Field> Fields { get; set; }
    public DbSet<VenueReview> VenueReviews { get; set; }
    public DbSet<VenueEvidence> VenueEvidences { get; set; }

    // Match Schema
    public DbSet<MatchRoom> MatchRooms { get; set; }
    public DbSet<MatchPreset> MatchPresets { get; set; }
    public DbSet<RoomParticipant> RoomParticipants { get; set; }
    public DbSet<RoomInvitation> RoomInvitations { get; set; }
    public DbSet<AfkVote> AfkVotes { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; }
    public DbSet<MatchFormation> MatchFormations { get; set; }
    public DbSet<FormationAssignment> FormationAssignments { get; set; }

    // Social Schema
    public DbSet<Post> Posts { get; set; }
    public DbSet<PostMedia> PostMedias { get; set; }
    public DbSet<PostLike> PostLikes { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<CommentLike> CommentLikes { get; set; }
    public DbSet<Friendship> Friendships { get; set; }

    // Evaluation Schema
    public DbSet<MatchFeedback> MatchFeedbacks { get; set; }
    public DbSet<EloHistory> EloHistories { get; set; }
    public DbSet<EloConfiguration> EloConfigurations { get; set; }
    public DbSet<PlayerRadarSnapshot> PlayerRadarSnapshots { get; set; }
    public DbSet<PlayerReport> PlayerReports { get; set; }
    public DbSet<ContentReport> ContentReports { get; set; }

    // System Schema
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<Announcement> Announcements { get; set; }
    public DbSet<SystemLog> SystemLogs { get; set; }

    // Payment Schema
    public DbSet<PaymentRequest> PaymentRequests { get; set; }
    public DbSet<Wallet> Wallets { get; set; }
    public DbSet<WalletTransaction> WalletTransactions { get; set; }
    public DbSet<WalletWithdrawal> WalletWithdrawals { get; set; }

    // Match Result Votes
    public DbSet<MatchResultVote> MatchResultVotes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Default);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        ApplySoftDeleteQueryFilter(modelBuilder);
        ApplyDependentEntityQueryFilters(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var utcNow = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = utcNow;
                    entry.Entity.UpdatedAt = utcNow;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = utcNow;
                    break;

                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    entry.Entity.DeletedAt = utcNow;
                    entry.Entity.UpdatedAt = utcNow;
                    break;
            }
        }

        int result = await base.SaveChangesAsync(cancellationToken);

        await PublishDomainEventsAsync();

        return result;
    }

    private async Task PublishDomainEventsAsync()
    {
        var domainEvents = ChangeTracker
            .Entries<Entity>()
            .Select(entry => entry.Entity)
            .SelectMany(entity =>
            {
                List<IDomainEvent> domainEvents = entity.DomainEvents;
                entity.ClearDomainEvents();
                return domainEvents;
            })
            .ToList();

        foreach (IDomainEvent domainEvent in domainEvents)
        {
            await publisher.Publish(domainEvent);
        }
    }

    private static void ApplySoftDeleteQueryFilter(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                continue;

            var parameter = Expression.Parameter(entityType.ClrType, "e");
            var property = Expression.Property(parameter, nameof(BaseEntity.DeletedAt));
            var condition = Expression.Equal(property, Expression.Constant(null));
            var lambda = Expression.Lambda(condition, parameter);

            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
        }
    }

    private static void ApplyDependentEntityQueryFilters(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ChatMessage>()
            .HasQueryFilter(e => e.MatchRoom == null || e.MatchRoom.DeletedAt == null);

        modelBuilder.Entity<CommentLike>()
            .HasQueryFilter(e => e.Comment.DeletedAt == null);

        modelBuilder.Entity<EloHistory>()
            .HasQueryFilter(e => e.Match.DeletedAt == null);

        modelBuilder.Entity<MatchFeedback>()
            .HasQueryFilter(e => e.Match.DeletedAt == null);

        modelBuilder.Entity<MatchPreset>()
            .HasQueryFilter(e => e.User.DeletedAt == null);

        modelBuilder.Entity<Notification>()
            .HasQueryFilter(e => e.User.DeletedAt == null);

        modelBuilder.Entity<PlayerAchievement>()
            .HasQueryFilter(e => e.User.DeletedAt == null && e.Achievement.DeletedAt == null);

        modelBuilder.Entity<PlayerReport>()
            .HasQueryFilter(e => e.Reporter.DeletedAt == null && e.Reported.DeletedAt == null);

        modelBuilder.Entity<WalletTransaction>()
            .HasQueryFilter(e => e.Wallet.DeletedAt == null);

        modelBuilder.Entity<WalletWithdrawal>()
            .HasQueryFilter(e => e.Wallet.DeletedAt == null);

        modelBuilder.Entity<PostLike>()
            .HasQueryFilter(e => e.Post.DeletedAt == null && e.User.DeletedAt == null);

        modelBuilder.Entity<PostMedia>()
            .HasQueryFilter(e => e.Post.DeletedAt == null);

        modelBuilder.Entity<RefreshToken>()
            .HasQueryFilter(e => e.User.DeletedAt == null);

        modelBuilder.Entity<RoomInvitation>()
            .HasQueryFilter(e => e.MatchRoom.DeletedAt == null && e.Inviter.DeletedAt == null && e.Invitee.DeletedAt == null);

        modelBuilder.Entity<RoomParticipant>()
            .HasQueryFilter(e => e.MatchRoom.DeletedAt == null && e.User.DeletedAt == null);

        modelBuilder.Entity<VenueOperatingHour>()
            .HasQueryFilter(e => e.Venue.DeletedAt == null);

        modelBuilder.Entity<VenuePhoto>()
            .HasQueryFilter(e => e.Venue.DeletedAt == null);

        modelBuilder.Entity<VenueReview>()
            .HasQueryFilter(e => e.DeletedAt == null && e.Venue.DeletedAt == null && e.User.DeletedAt == null && e.Booking.DeletedAt == null);
    }
}
