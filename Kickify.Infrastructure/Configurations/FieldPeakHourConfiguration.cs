using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kickify.Infrastructure.Configurations;

public class FieldPeakHourConfiguration : IEntityTypeConfiguration<FieldPeakHour>
{
    public void Configure(EntityTypeBuilder<FieldPeakHour> builder)
    {
        builder.ToTable("FieldPeakHours", Schemas.Venue);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.FieldId)
            .IsRequired();

        builder.Property(x => x.StartTime)
            .HasColumnType("time")
            .IsRequired();

        builder.Property(x => x.EndTime)
            .HasColumnType("time")
            .IsRequired();

        builder.Property(x => x.SurchargeAmount)
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        builder.Property(x => x.IsPercentage)
            .HasDefaultValue(false)
            .IsRequired();

        var applicableDaysComparer = new ValueComparer<List<DayOfWeekEnum>>(
            (left, right) =>
                ReferenceEquals(left, right) ||
                (left != null && right != null && left.SequenceEqual(right)),
            list =>
                list == null
                    ? 0
                    : list.Aggregate(0, (current, item) => HashCode.Combine(current, item.GetHashCode())),
            list => list == null ? new List<DayOfWeekEnum>() : list.ToList());

        builder.Property(x => x.ApplicableDays)
            .HasColumnType("integer[]")
            .HasConversion(
                v => v.Select(d => (int)d).ToArray(),
                v => v.Select(d => (DayOfWeekEnum)d).ToList())
            .Metadata.SetValueComparer(applicableDaysComparer);

        builder.HasIndex(x => x.FieldId);
    }
}
