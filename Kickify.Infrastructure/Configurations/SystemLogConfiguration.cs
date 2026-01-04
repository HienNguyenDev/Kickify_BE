using Kickify.Domain.Entities;
using Kickify.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kickify.Infrastructure.Configurations;

public class SystemLogConfiguration : IEntityTypeConfiguration<SystemLog>
{
    public void Configure(EntityTypeBuilder<SystemLog> builder)
    {
        builder.ToTable("SystemLogs", Schemas.System);

        builder.HasKey(sl => sl.LogId);

        builder.Property(sl => sl.LogId)
            .IsRequired();

        builder.Property(sl => sl.UserId);

        builder.Property(sl => sl.Action)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(sl => sl.EntityType)
            .HasMaxLength(100);

        builder.Property(sl => sl.EntityId);

        builder.Property(sl => sl.IpAddress)
            .HasMaxLength(45);

        builder.Property(sl => sl.UserAgent);

        builder.Property(sl => sl.RequestDetails)
            .HasColumnType("text")
            .HasComment("JSON");

        builder.Property(sl => sl.ResponseStatus);

        builder.Property(sl => sl.ErrorMessage)
            .HasColumnType("text");

        builder.Property(sl => sl.CreatedAt);

        // Indexes
        builder.HasIndex(sl => sl.UserId);
        builder.HasIndex(sl => sl.Action);
        builder.HasIndex(sl => sl.CreatedAt);
        builder.HasIndex(sl => sl.EntityType);
    }
}
