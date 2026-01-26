using Kickify.Domain.Entities;
using Kickify.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kickify.Infrastructure.Configurations;

public class PlayerWalletConfiguration : IEntityTypeConfiguration<PlayerWallet>
{
    public void Configure(EntityTypeBuilder<PlayerWallet> builder)
    {
        builder.ToTable("PlayerWallets", Schemas.Payment);
        builder.HasKey(pw => pw.PlayerWalletId);

        builder.Property(pw => pw.Balance).HasPrecision(18, 2).HasDefaultValue(0);
        builder.Property(pw => pw.BankAccountNumber).HasMaxLength(50);
        builder.Property(pw => pw.BankName).HasMaxLength(100);
        builder.Property(pw => pw.AccountHolderName).HasMaxLength(200);

        builder.HasOne(pw => pw.User).WithOne(u => u.PlayerWallet).HasForeignKey<PlayerWallet>(pw => pw.UserId).OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(pw => pw.UserId).IsUnique();
    }
}
