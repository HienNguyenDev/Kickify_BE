using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kickify.Infrastructure.Configurations;

public class PlayerWithdrawalConfiguration : IEntityTypeConfiguration<PlayerWithdrawal>
{
    public void Configure(EntityTypeBuilder<PlayerWithdrawal> builder)
    {
        builder.ToTable("PlayerWithdrawals", Schemas.Payment);
        builder.HasKey(w => w.PlayerWithdrawalId);

        builder.Property(w => w.Amount).HasPrecision(18, 2).IsRequired();
        builder.Property(w => w.Status).HasConversion<string>().HasDefaultValue(WithdrawalStatus.Pending).IsRequired();
        builder.Property(w => w.RequestDate).HasDefaultValueSql("NOW()");
        builder.Property(w => w.AdminNotes).HasMaxLength(1000);

        builder.HasOne(w => w.PlayerWallet).WithMany(pw => pw.Withdrawals).HasForeignKey(w => w.PlayerWalletId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(w => w.ProcessedByAdmin).WithMany().HasForeignKey(w => w.ProcessedByAdminId).OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(w => w.PlayerWalletId);
        builder.HasIndex(w => w.Status);
    }
}
