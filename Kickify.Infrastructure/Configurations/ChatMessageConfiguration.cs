using Kickify.Domain.Entities;
using Kickify.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kickify.Infrastructure.Configurations;

public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.ToTable("ChatMessages", Schemas.Social);

        builder.HasKey(cm => cm.MessageId);

        builder.Property(cm => cm.MessageId)
            .IsRequired();

        builder.Property(cm => cm.RoomId)
            .IsRequired();

        builder.Property(cm => cm.SenderId)
            .IsRequired();

        builder.Property(cm => cm.MessageText)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(cm => cm.MessageType)
            .HasConversion<string>()
            .HasDefaultValue(Domain.Enums.MessageType.Text);

        builder.Property(cm => cm.IsEdited)
            .HasDefaultValue(false);

        // Indexes
        builder.HasIndex(cm => cm.RoomId);
        builder.HasIndex(cm => cm.SentAt);
    }
}
