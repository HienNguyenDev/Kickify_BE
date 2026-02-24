using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kickify.Infrastructure.Configurations;

public class WalletTransactionConfiguration : IEntityTypeConfiguration<WalletTransaction>
{
    public void Configure(EntityTypeBuilder<WalletTransaction> builder)
    {
        builder.ToTable("WalletTransactions", Schemas.Payment);

        builder.HasKey(t => t.TransactionId);

        builder.Property(t => t.TransactionId).IsRequired();
        builder.Property(t => t.WalletId).IsRequired();
        builder.Property(t => t.TransactionType).HasConversion<string>().IsRequired();
        builder.Property(t => t.Amount).HasPrecision(18, 2).IsRequired();
        builder.Property(t => t.BalanceAfter).HasPrecision(18, 2).IsRequired();
        builder.Property(t => t.TransactionCode).HasMaxLength(100);
        builder.Property(t => t.Description).HasMaxLength(500);
        builder.Property(t => t.CreatedAt).IsRequired();

        builder.HasIndex(t => t.WalletId);
        builder.HasIndex(t => t.TransactionCode);
        builder.HasIndex(t => t.CreatedAt);
    }
}
