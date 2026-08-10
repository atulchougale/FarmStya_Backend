using FarmStay.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmStay.Infrastructure.Persistence.Configurations
{
    public class UserRefreshTokenConfiguration : IEntityTypeConfiguration<UserRefreshToken>
    {
        public void Configure(EntityTypeBuilder<UserRefreshToken> builder)
        {
            builder.ToTable("UserRefreshTokens");

            builder.HasKey(x => x.UserRefreshTokenId);

            builder.Property(x => x.RefreshTokenHash)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(x => x.DeviceName)
                .HasMaxLength(100);

            builder.Property(x => x.Browser)
                .HasMaxLength(100);

            builder.Property(x => x.OperatingSystem)
                .HasMaxLength(100);

            builder.Property(x => x.IPAddress)
                .HasMaxLength(100);

            // User -> RefreshTokens (1 : Many)
            builder.HasOne(x => x.User)
                .WithMany(x => x.UserRefreshTokens)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Self Reference (Token Rotation)
            builder.HasOne(x => x.ReplacedByToken)
                .WithMany(x => x.ChildTokens)
                .HasForeignKey(x => x.ReplacedByTokenId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.RefreshTokenHash)
                .IsUnique();

            builder.HasIndex(x => x.UserId);

            builder.HasIndex(x => x.ExpiryDate);
        }
    }
}