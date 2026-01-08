using Kickify.Domain.Entities;
using Kickify.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kickify.Infrastructure.Configurations
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ToTable("RefreshTokens", Schemas.Identity);
            builder.HasKey(r => r.TokenId);

            builder.Property(r => r.Token)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(r => r.ExpiresAt)
                   .IsRequired();   

            builder.Property(r => r.UserId)
                   .IsRequired();

            builder.HasIndex(r => r.Token)
                   .IsUnique();

            builder.HasOne(r => r.User)
                   .WithMany(u => u.RefreshTokens)   
                   .HasForeignKey(r => r.UserId)
                   .OnDelete(DeleteBehavior.Cascade); 
        }
    }
}