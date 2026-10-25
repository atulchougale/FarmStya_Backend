
using FarmStay.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmStay.Infrastructure.Persistence.Configurations
{
    public class BookingActionTokenConfiguration
        : IEntityTypeConfiguration<BookingActionToken>
    {
        public void Configure(
            EntityTypeBuilder<BookingActionToken> builder)
        {
            builder.ToTable("BookingActionTokens");

            builder.HasKey(x => x.BookingActionTokenId);

            builder.Property(x => x.BookingActionTokenId)
                .UseIdentityColumn();

            builder.Property(x => x.TokenHash)
                .HasMaxLength(64)
                .IsUnicode(false)
                .IsRequired();

            builder.Property(x => x.Purpose)
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(x => x.ExpiresAtUtc)
                .HasColumnType("datetime2(0)")
                .IsRequired();

            builder.Property(x => x.ConsumedAtUtc)
                .HasColumnType("datetime2(0)");

            builder.Property(x => x.CreatedDate)
                .HasColumnType("datetime2(0)")
                .IsRequired();

            builder.Property(x => x.ModifiedDate)
                .HasColumnType("datetime2(0)");

            builder.Property(x => x.IsDelete)
                .HasDefaultValue(false);

            builder.HasOne(x => x.FarmHouse)
                .WithMany()
                .HasForeignKey(x => x.FarmHouseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Booking)
                .WithMany()
                .HasForeignKey(x => x.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.TokenHash)
                .IsUnique();

            builder.HasIndex(x => new
            {
                x.FarmHouseId,
                x.BookingId,
                x.Purpose
            });

            builder.HasIndex(x => new
            {
                x.ExpiresAtUtc,
                x.ConsumedAtUtc
            });
        }
    }
}
