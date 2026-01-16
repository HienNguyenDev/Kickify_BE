using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
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
            .IsRequired(false);

        builder.Property(cm => cm.ReceiverId)
            .IsRequired(false);

        builder.Property(cm => cm.SenderId)
            .IsRequired();

        builder.Property(cm => cm.ConversationType)
            .HasConversion<string>()
            .HasDefaultValue(ConversationType.Private)
            .IsRequired();

        builder.Property(cm => cm.MessageText)
            .HasColumnType("text") 
            .IsRequired();

        builder.Property(cm => cm.MessageType)
            .HasConversion<string>()
            .HasDefaultValue(MessageType.Text)
            .IsRequired();

        builder.Property(cm => cm.IsEdited)
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(cm => cm.IsRead)
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(cm => cm.SentAt)
            .HasColumnType("timestamp with time zone") 
            .IsRequired();

        // ⭐ Relationships
        builder.HasOne(cm => cm.Sender)
            .WithMany()
            .HasForeignKey(cm => cm.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(cm => cm.Receiver)
            .WithMany()
            .HasForeignKey(cm => cm.ReceiverId)
            .OnDelete(DeleteBehavior.Restrict);

        //builder.HasOne(cm => cm.MatchRoom)
        //    .WithMany(r => r.Messages)
        //    .HasForeignKey(cm => cm.RoomId)
        //    .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(cm => cm.RoomId);
        builder.HasIndex(cm => cm.SenderId);
        builder.HasIndex(cm => cm.ReceiverId);
        builder.HasIndex(cm => cm.SentAt);

        builder.HasIndex(cm => new { cm.SenderId, cm.ReceiverId, cm.SentAt });

        builder.HasIndex(cm => new { cm.ReceiverId, cm.IsRead })
            .HasFilter("\"ReceiverId\" IS NOT NULL AND \"IsRead\" = false");
    }
}
