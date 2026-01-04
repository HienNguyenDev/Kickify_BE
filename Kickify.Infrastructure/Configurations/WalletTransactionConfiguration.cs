using Kickify.Domain.Entities;
using Kickify.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kickify.Infrastructure.Configurations;

public class WalletTransactionConfiguration : IEntityTypeConfiguration<WalletTransaction>
{
    public void Configure(EntityTypeBuilder<WalletTransaction> builder)
    {
        builder.ToTable("WalletTransactions", Schemas.Venue);

        builder.HasKey(wt => wt.TransactionId);

        builder.Property(wt => wt.TransactionId)
            .IsRequired();

        builder.Property(wt => wt.WalletId)
            .IsRequired();

        builder.Property(wt => wt.TransactionType)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(wt => wt.Amount)
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        builder.Property(wt => wt.BalanceAfter)
            .HasColumnType("decimal(12,2)")
            .IsRequired();

        builder.Property(wt => wt.ReferenceId)
            .HasComment("booking_id or withdrawal_id");

        builder.Property(wt => wt.Description)
            .HasColumnType("text");

        // Indexes
        builder.HasIndex(wt => wt.WalletId);
        builder.HasIndex(wt => wt.CreatedAt);
    }
}
