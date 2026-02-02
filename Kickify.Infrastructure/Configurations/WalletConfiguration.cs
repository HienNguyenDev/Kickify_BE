using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kickify.Infrastructure.Configurations;

public class WalletConfiguration : IEntityTypeConfiguration<Wallet>
{
    public void Configure(EntityTypeBuilder<Wallet> builder)
    {
        builder.ToTable("Wallets", Schemas.Payment);

        builder.HasKey(w => w.WalletId);

        builder.Property(w => w.WalletId).IsRequired();
        builder.Property(w => w.UserId).IsRequired();
        builder.Property(w => w.WalletType).HasConversion<string>().IsRequired();
        builder.Property(w => w.Balance).HasPrecision(18, 2).HasDefaultValue(0);
        builder.Property(w => w.BankAccountNumber).HasMaxLength(50);
        builder.Property(w => w.BankName).HasMaxLength(100);
        builder.Property(w => w.AccountHolderName).HasMaxLength(100);
        builder.Property(w => w.IsBankVerified).HasDefaultValue(false);

        builder.HasIndex(w => w.UserId).IsUnique();

        builder.HasOne(w => w.User)
            .WithOne(u => u.Wallet)
            .HasForeignKey<Wallet>(w => w.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(w => w.Transactions)
            .WithOne(t => t.Wallet)
            .HasForeignKey(t => t.WalletId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(w => w.Withdrawals)
            .WithOne(wd => wd.Wallet)
            .HasForeignKey(wd => wd.WalletId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
