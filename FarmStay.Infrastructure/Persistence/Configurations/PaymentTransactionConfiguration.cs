
using FarmStay.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmStay.Infrastructure.Persistence.Configurations
{
    public class PaymentTransactionConfiguration
        : IEntityTypeConfiguration<PaymentTransaction>
    {
        public void Configure(EntityTypeBuilder<PaymentTransaction> builder)
        {
            builder.ToTable("PaymentTransactions");

            builder.HasKey(p => p.PaymentTransactionId);

            builder.Property(p => p.PaymentTransactionId)
                .ValueGeneratedOnAdd();

            // Tenant
            builder.Property(p => p.FarmHouseId)
                .IsRequired();

            builder.HasOne(p => p.FarmHouse)
                .WithMany()
                .HasForeignKey(p => p.FarmHouseId)
                .OnDelete(DeleteBehavior.Restrict);

            // Booking
            builder.Property(p => p.BookingId)
                .IsRequired();

            builder.HasOne(p => p.Booking)
                .WithMany()
                .HasForeignKey(p => p.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            // Payment type and method
            builder.Property(p => p.PaymentType)
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(p => p.PaymentMethod)
                .HasMaxLength(30)
                .IsRequired();

            // Amount and currency
            builder.Property(p => p.Amount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(p => p.CurrencyCode)
                .HasColumnType("char(3)")
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasDefaultValue("INR")
                .IsRequired();

            // Status
            builder.Property(p => p.PaymentStatus)
                .HasMaxLength(30)
                .IsRequired();

            // Gateway details
            builder.Property(p => p.PaymentProvider)
                .HasMaxLength(50);

            builder.Property(p => p.ProviderOrderId)
                .HasMaxLength(150);

            builder.Property(p => p.ProviderPaymentId)
                .HasMaxLength(150);

            builder.Property(p => p.ProviderRefundId)
                .HasMaxLength(150);

            // Idempotency
            builder.Property(p => p.IdempotencyKey)
                .HasMaxLength(150)
                .IsRequired();

            builder.HasIndex(p => new
            {
                p.FarmHouseId,
                p.IdempotencyKey
            })
            .IsUnique();

            // Payment timestamps
            builder.Property(p => p.PaidAtUtc)
                .HasColumnType("datetime2(0)");

            builder.Property(p => p.VerifiedAtUtc)
                .HasColumnType("datetime2(0)");

            builder.Property(p => p.FailureReason)
                .HasMaxLength(1000);

            // Audit
            builder.Property(p => p.IsDelete)
                .HasDefaultValue(false)
                .IsRequired();

            builder.Property(p => p.CreatedBy)
                .IsRequired();

            builder.Property(p => p.CreatedDate)
                .HasColumnType("datetime2(0)")
                .IsRequired();

            builder.Property(p => p.ModifiedBy);

            builder.Property(p => p.ModifiedDate)
                .HasColumnType("datetime2(0)");

            // Payment lookup
            builder.HasIndex(p => new
            {
                p.FarmHouseId,
                p.BookingId,
                p.PaymentStatus
            });
        }
    }
}
