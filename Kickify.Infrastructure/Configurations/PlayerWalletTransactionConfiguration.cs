using Kickify.Domain.Entities;
using Kickify.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kickify.Infrastructure.Configurations;

public class PlayerWalletTransactionConfiguration : IEntityTypeConfiguration<PlayerWalletTransaction>
{
    public void Configure(EntityTypeBuilder<PlayerWalletTransaction> builder)
    {
        builder.ToTable("PlayerWalletTransactions", Schemas.Payment);
        builder.HasKey(t => t.TransactionId);

        builder.Property(t => t.TransactionType).HasConversion<string>().IsRequired();
        builder.Property(t => t.Amount).HasPrecision(18, 2).IsRequired();
        builder.Property(t => t.BalanceAfter).HasPrecision(18, 2).IsRequired();
        builder.Property(t => t.Description).HasMaxLength(500);
        builder.Property(t => t.CreatedAt).HasDefaultValueSql("NOW()");

        builder.HasOne(t => t.PlayerWallet).WithMany(pw => pw.Transactions).HasForeignKey(t => t.PlayerWalletId).OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(t => t.PlayerWalletId);
        builder.HasIndex(t => t.CreatedAt);
    }
}
