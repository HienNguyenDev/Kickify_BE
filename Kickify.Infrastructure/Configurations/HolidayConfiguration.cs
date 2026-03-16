using Kickify.Domain.Entities;
using Kickify.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kickify.Infrastructure.Configurations;

public class HolidayConfiguration : IEntityTypeConfiguration<Holiday>
{
    public void Configure(EntityTypeBuilder<Holiday> builder)
    {
        builder.ToTable("Holidays", Schemas.System);

        builder.HasKey(h => h.Id);

        builder.Property(h => h.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(h => h.Date)
            .HasColumnType("date")
            .IsRequired();

        builder.HasIndex(h => h.Date)
            .IsUnique();
    }
}