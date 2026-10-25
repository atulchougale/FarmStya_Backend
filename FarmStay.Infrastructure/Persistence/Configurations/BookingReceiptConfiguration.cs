
using FarmStay.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmStay.Infrastructure.Persistence.Configurations
{
    public class BookingReceiptConfiguration
        : IEntityTypeConfiguration<BookingReceipt>
    {
        public void Configure(EntityTypeBuilder<BookingReceipt> builder)
        {
            // Table
            builder.ToTable("BookingReceipts");

            // Primary Key
            builder.HasKey(r => r.ReceiptId);

            builder.Property(r => r.ReceiptId)
                .ValueGeneratedOnAdd();

            // FarmHouse
            builder.Property(r => r.FarmHouseId)
                .IsRequired();

            builder.HasOne(r => r.FarmHouse)
                .WithMany()
                .HasForeignKey(r => r.FarmHouseId)
                .OnDelete(DeleteBehavior.Restrict);

            // Booking
            builder.Property(r => r.BookingId)
                .IsRequired();

            builder.HasOne(r => r.Booking)
                .WithMany()
                .HasForeignKey(r => r.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            // Payment Transaction
            builder.Property(r => r.PaymentTransactionId)
                .IsRequired();

            builder.HasOne(r => r.PaymentTransaction)
                .WithMany()
                .HasForeignKey(r => r.PaymentTransactionId)
                .OnDelete(DeleteBehavior.Restrict);

            // Receipt Number
            builder.Property(r => r.ReceiptNumber)
                .HasMaxLength(50)
                .IsUnicode()
                .IsRequired();

            builder.HasIndex(r => r.ReceiptNumber)
                .IsUnique();

            // Amount
            builder.Property(r => r.Amount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            // Currency
            builder.Property(r => r.CurrencyCode)
                .HasColumnType("char(3)")
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasDefaultValue("INR")
                .IsRequired();

            // Issued timestamp
            builder.Property(r => r.IssuedAtUtc)
                .HasColumnType("datetime2(0)")
                .IsRequired();

            // Audit
            builder.Property(r => r.CreatedBy)
                .IsRequired();

            builder.Property(r => r.CreatedDate)
                .HasColumnType("datetime2(0)")
                .IsRequired();

            builder.Property(r => r.ModifiedBy)
                .IsRequired(false);

            builder.Property(r => r.ModifiedDate)
                .HasColumnType("datetime2(0)")
                .IsRequired(false);

            // Receipt lookup
            builder.HasIndex(r => new
            {
                r.FarmHouseId,
                r.BookingId
            });
        }
    }
}
