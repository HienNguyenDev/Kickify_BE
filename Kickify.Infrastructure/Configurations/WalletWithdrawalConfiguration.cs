using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kickify.Infrastructure.Configurations;

public class WalletWithdrawalConfiguration : IEntityTypeConfiguration<WalletWithdrawal>
{
    public void Configure(EntityTypeBuilder<WalletWithdrawal> builder)
    {
        builder.ToTable("WalletWithdrawals", Schemas.Payment);

        builder.HasKey(w => w.WithdrawalId);

        builder.Property(w => w.WithdrawalId).IsRequired();
        builder.Property(w => w.WalletId).IsRequired();
        builder.Property(w => w.Amount).HasPrecision(18, 2).IsRequired();
        builder.Property(w => w.Status).HasConversion<string>().HasDefaultValue(WithdrawalStatus.Pending);
        builder.Property(w => w.RequestDate).IsRequired();
        builder.Property(w => w.AdminNotes).HasMaxLength(500);

        builder.HasIndex(w => w.WalletId);
        builder.HasIndex(w => w.Status);
        builder.HasIndex(w => w.RequestDate);

        builder.HasOne(w => w.ProcessedByAdmin)
            .WithMany()
            .HasForeignKey(w => w.ProcessedByAdminId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
