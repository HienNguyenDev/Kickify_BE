using Kickify.Application.Abstractions.Persistence;
using Kickify.Domain.Common;
using Kickify.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Kickify.Infrastructure.Database
{
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
        public DbSet<VenuePhoto> VenuePhotos { get; set; }
        public DbSet<VenueOperatingHour> VenueOperatingHours { get; set; }
        public DbSet<Field> Fields { get; set; }
        public DbSet<VenueWallet> VenueWallets { get; set; }
        public DbSet<WalletTransaction> WalletTransactions { get; set; }
        public DbSet<Withdrawal> Withdrawals { get; set; }
        public DbSet<VenueReview> VenueReviews { get; set; }

        // Match Schema
        public DbSet<MatchRoom> MatchRooms { get; set; }
        public DbSet<MatchPreset> MatchPresets { get; set; }
        public DbSet<RoomParticipant> RoomParticipants { get; set; }
        public DbSet<RoomInvitation> RoomInvitations { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }

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
        public DbSet<PlayerReport> PlayerReports { get; set; }

        // System Schema
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<SystemLog> SystemLogs { get; set; }

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

            // When should you publish domain events?
            //
            // 1. BEFORE calling SaveChangesAsync
            //     - domain events are part of the same transaction
            //     - immediate consistency
            // 2. AFTER calling SaveChangesAsync
            //     - domain events are a separate transaction
            //     - eventual consistency
            //     - handlers can fail

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
            // ChatMessage → MatchRoom
            modelBuilder.Entity<ChatMessage>()
                .HasQueryFilter(e => e.MatchRoom.DeletedAt == null);

            // EloHistory → MatchRoom
            modelBuilder.Entity<EloHistory>()
                .HasQueryFilter(e => e.Match.DeletedAt == null);

            // MatchFeedback → MatchRoom
            modelBuilder.Entity<MatchFeedback>()
                .HasQueryFilter(e => e.Match.DeletedAt == null);

            // MatchPreset → User
            modelBuilder.Entity<MatchPreset>()
                .HasQueryFilter(e => e.User.DeletedAt == null);

            // Notification → User
            modelBuilder.Entity<Notification>()
                .HasQueryFilter(e => e.User.DeletedAt == null);

            // PlayerAchievement → User, Achievement
            modelBuilder.Entity<PlayerAchievement>()
                .HasQueryFilter(e => e.User.DeletedAt == null && e.Achievement.DeletedAt == null);

            // PlayerReport → User (Reporter and Reported)
            modelBuilder.Entity<PlayerReport>()
                .HasQueryFilter(e => e.Reporter.DeletedAt == null && e.Reported.DeletedAt == null);

            // PostLike → Post, User
            modelBuilder.Entity<PostLike>()
                .HasQueryFilter(e => e.Post.DeletedAt == null && e.User.DeletedAt == null);

            // PostMedia → Post
            modelBuilder.Entity<PostMedia>()
                .HasQueryFilter(e => e.Post.DeletedAt == null);

            // RoomInvitation → MatchRoom, User (Inviter and Invitee)
            modelBuilder.Entity<RoomInvitation>()
                .HasQueryFilter(e => e.MatchRoom.DeletedAt == null && e.Inviter.DeletedAt == null && e.Invitee.DeletedAt == null);

            // RoomParticipant → MatchRoom, User
            modelBuilder.Entity<RoomParticipant>()
                .HasQueryFilter(e => e.MatchRoom.DeletedAt == null && e.User.DeletedAt == null);

            // VenueOperatingHour → Venue
            modelBuilder.Entity<VenueOperatingHour>()
                .HasQueryFilter(e => e.Venue.DeletedAt == null);

            // VenuePhoto → Venue
            modelBuilder.Entity<VenuePhoto>()
                .HasQueryFilter(e => e.Venue.DeletedAt == null);

            // VenueReview → Venue, User, Booking
            modelBuilder.Entity<VenueReview>()
                .HasQueryFilter(e => e.Venue.DeletedAt == null && e.User.DeletedAt == null && e.Booking.DeletedAt == null);

            // WalletTransaction → VenueWallet
            modelBuilder.Entity<WalletTransaction>()
                .HasQueryFilter(e => e.VenueWallet.DeletedAt == null);

            // Withdrawal → VenueWallet
            modelBuilder.Entity<Withdrawal>()
                .HasQueryFilter(e => e.VenueWallet.DeletedAt == null);
        }
    }
}
