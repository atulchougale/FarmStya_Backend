
using FarmStay.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmStay.Infrastructure.Persistence.Configurations
{
    public class CheckoutDiscountConfiguration
        : IEntityTypeConfiguration<CheckoutDiscount>
    {
        public void Configure(
            EntityTypeBuilder<CheckoutDiscount> builder)
        {
            builder.ToTable("CheckoutDiscounts");

            builder.HasKey(x => x.CheckoutDiscountId);

            builder.Property(x => x.CheckoutDiscountId)
                .UseIdentityColumn();

            builder.Property(x => x.Amount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(x => x.Reason)
                .HasMaxLength(1000)
                .IsRequired();

            builder.Property(x => x.CreatedDate)
                .HasColumnType("datetime2(0)")
                .IsRequired();

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

            builder.HasIndex(x => new
            {
                x.FarmHouseId,
                x.BookingId,
                x.IsDelete
            });
        }
    }
}
