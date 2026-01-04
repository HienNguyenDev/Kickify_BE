using Kickify.Domain.Entities;
using Kickify.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kickify.Infrastructure.Configurations;

public class VenuePhotoConfiguration : IEntityTypeConfiguration<VenuePhoto>
{
    public void Configure(EntityTypeBuilder<VenuePhoto> builder)
    {
        builder.ToTable("VenuePhotos", Schemas.Venue);

        builder.HasKey(vp => vp.PhotoId);

        builder.Property(vp => vp.PhotoId)
            .IsRequired();

        builder.Property(vp => vp.VenueId)
            .IsRequired();

        builder.Property(vp => vp.PhotoUrl)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(vp => vp.DisplayOrder)
            .HasDefaultValue(0);
    }
}
