using Kickify.Domain.Entities;
using Kickify.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kickify.Infrastructure.Configurations;

public class NotificationPreferenceConfiguration : IEntityTypeConfiguration<NotificationPreference>
{
    public void Configure(EntityTypeBuilder<NotificationPreference> builder)
    {
        builder.ToTable("NotificationPreferences", Schemas.Identity);

        builder.HasKey(n => n.PreferenceId);

        builder.Property(n => n.PreferenceId)
            .IsRequired();

        builder.Property(np => np.UserId)
            .IsRequired();

        builder.Property(n => n.MatchRoom)
            .HasDefaultValue(true);

        builder.Property(n => n.Friendship)
            .HasDefaultValue(true);

        builder.Property(n => n.Post)
            .HasDefaultValue(true);
    }
}
