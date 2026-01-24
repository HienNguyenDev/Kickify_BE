using Kickify.Domain.Entities;
using Kickify.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kickify.Infrastructure.Configurations;

public class VenueWalletConfiguration : IEntityTypeConfiguration<VenueWallet>
{
    public void Configure(EntityTypeBuilder<VenueWallet> builder)
    {
        builder.ToTable("VenueWallets", Schemas.Venue);
        builder.HasKey(vw => vw.VenueWalletId);

        builder.Property(vw => vw.Balance)
            .HasColumnType("decimal(12,2)")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(vw => vw.BankAccountNumber)
            .HasMaxLength(50);

        builder.Property(vw => vw.BankName)
            .HasMaxLength(100);

        builder.Property(vw => vw.AccountHolderName)
            .HasMaxLength(100);

        builder.Property(vw => vw.IsBankVerified)
            .HasDefaultValue(false);

        // Relationships
        builder.HasMany(vw => vw.WalletTransactions)
            .WithOne(wt => wt.VenueWallet)
            .HasForeignKey(wt => wt.VenueWalletId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(vw => vw.VenueWithdrawals)
            .WithOne(w => w.VenueWallet)
            .HasForeignKey(w => w.VenueWalletId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
