using FarmStay.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmStay.Infrastructure.Persistence.Configurations
{
    public class UserOtpConfiguration : IEntityTypeConfiguration<UserOtp>
    {
        public void Configure(EntityTypeBuilder<UserOtp> builder)
        {
            // Table
            builder.ToTable("UserOtps");

            // Primary Key
            builder.HasKey(x => x.UserOtpId);

            // Properties
            builder.Property(x => x.MobileNumber)
                .IsRequired()
                .HasMaxLength(10);

            builder.Property(x => x.OtpCode)
                .IsRequired()
                .HasMaxLength(6);

            builder.Property(x => x.Purpose)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(x => x.ExpiryDate)
                .IsRequired();

            builder.Property(x => x.IsUsed)
                .HasDefaultValue(false);

            builder.Property(x => x.IsActive)
                .HasDefaultValue(true);

            builder.Property(x => x.IsDeleted)
                .HasDefaultValue(false);

            builder.Property(x => x.CreatedDate)
                .IsRequired();

            // Foreign Key
            builder.HasOne(x => x.User)
                .WithMany(x => x.UserOtps)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(x => x.UserId);

            builder.HasIndex(x => x.MobileNumber);

            builder.HasIndex(x => new
            {
                x.UserId,
                x.Purpose,
                x.IsUsed
            });

            builder.HasIndex(x => x.ExpiryDate);
        }
    }
}