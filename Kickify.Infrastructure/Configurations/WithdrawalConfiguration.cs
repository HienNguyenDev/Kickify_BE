using Kickify.Domain.Entities;
using Kickify.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kickify.Infrastructure.Configurations;

public class WithdrawalConfiguration : IEntityTypeConfiguration<Withdrawal>
{
    public void Configure(EntityTypeBuilder<Withdrawal> builder)
    {
        builder.ToTable("Withdrawals", Schemas.Venue);

        builder.HasKey(w => w.WithdrawalId);

        builder.Property(w => w.WithdrawalId)
            .IsRequired();

        builder.Property(w => w.WalletId)
            .IsRequired();

        builder.Property(w => w.Amount)
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        builder.Property(w => w.Status)
            .HasConversion<string>()
            .HasDefaultValue(Domain.Enums.WithdrawalStatus.Pending);

        builder.Property(w => w.ProcessedDate)
            .HasColumnType("timestamp");

        builder.Property(w => w.AdminNotes)
            .HasColumnType("text");
    }
}
