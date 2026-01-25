using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kickify.Infrastructure.Configurations;

public class VenueWithdrawalConfiguration : IEntityTypeConfiguration<VenueWithdrawal>
{
    public void Configure(EntityTypeBuilder<VenueWithdrawal> builder)
    {
        builder.ToTable("VenueWithdrawals", Schemas.Venue);
        builder.HasKey(w => w.VenueWithdrawalId);

        builder.Property(w => w.Amount)
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        builder.Property(w => w.Status)
            .HasConversion<string>()
            .HasDefaultValue(WithdrawalStatus.Pending);

        builder.Property(w => w.ProcessedDate)
            .HasColumnType("timestamp");

        builder.Property(w => w.AdminNotes)
            .HasColumnType("text");

        builder.HasOne(w => w.ProcessedByAdmin)
            .WithMany()
            .HasForeignKey(w => w.ProcessedByAdminId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(w => w.VenueWalletId);
        builder.HasIndex(w => w.Status);
    }
}
