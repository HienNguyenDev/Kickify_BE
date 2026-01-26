using Kickify.Domain.Entities;
using Kickify.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kickify.Infrastructure.Configurations;

public class VenueWalletTransactionConfiguration : IEntityTypeConfiguration<VenueWalletTransaction>
{
    public void Configure(EntityTypeBuilder<VenueWalletTransaction> builder)
    {
        builder.ToTable("VenueWalletTransactions", Schemas.Payment);

        builder.HasKey(t => t.TransactionId);

        builder.Property(t => t.TransactionId)
            .IsRequired();

        builder.Property(t => t.VenueWalletId)
            .IsRequired();

        builder.Property(t => t.TransactionType)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(t => t.Amount)
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        builder.Property(t => t.BalanceAfter)
            .HasColumnType("decimal(12,2)")
            .IsRequired();

        builder.Property(t => t.TransactionCode)
            .HasMaxLength(100);

        builder.Property(t => t.ReferenceId)
            .HasComment("booking_id or withdrawal_id");

        builder.Property(t => t.Description)
            .HasColumnType("text");

        // Indexes
        builder.HasIndex(t => t.VenueWalletId);
        builder.HasIndex(t => t.TransactionCode);
        builder.HasIndex(t => t.CreatedAt);
    }
}
