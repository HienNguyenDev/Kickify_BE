using Kickify.Domain.Entities;
using Kickify.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kickify.Infrastructure.Configurations;

public class PaymentRequestConfiguration : IEntityTypeConfiguration<PaymentRequest>
{
    public void Configure(EntityTypeBuilder<PaymentRequest> builder)
    {
        builder.ToTable("PaymentRequests", Schemas.Payment);

        builder.HasKey(e => e.PaymentRequestId);

        builder.Property(e => e.PaymentRequestId).ValueGeneratedNever();
        builder.Property(e => e.TxnRef).IsRequired().HasMaxLength(50);
        builder.Property(e => e.Amount).IsRequired().HasPrecision(18, 2);
        builder.Property(e => e.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.VnpayTransactionNo).HasMaxLength(100);
        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.ExpiredAt).IsRequired();

        builder.HasIndex(e => e.TxnRef).IsUnique();
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.WalletId);
        builder.HasIndex(e => e.Status);

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Wallet)
            .WithMany()
            .HasForeignKey(e => e.WalletId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
