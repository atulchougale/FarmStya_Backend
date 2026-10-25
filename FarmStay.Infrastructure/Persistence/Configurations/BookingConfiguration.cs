
using FarmStay.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmStay.Infrastructure.Persistence.Configurations
{
    public class BookingConfiguration : IEntityTypeConfiguration<Booking>
    {
        public void Configure(EntityTypeBuilder<Booking> builder)
        {
            // ================= Table =================

            builder.ToTable("Bookings");

            // ================= Primary Key =================

            builder.HasKey(b => b.BookingId);

            builder.Property(b => b.BookingId)
                .ValueGeneratedOnAdd();

            // ================= Tenant =================

            builder.Property(b => b.FarmHouseId)
                .IsRequired();

            builder.HasOne(b => b.FarmHouse)
                .WithMany()
                .HasForeignKey(b => b.FarmHouseId)
                .OnDelete(DeleteBehavior.Restrict);

            // ================= Customer =================

            builder.Property(b => b.CustomerUserId)
                .IsRequired();

            builder.HasOne(b => b.CustomerUser)
                .WithMany()
                .HasForeignKey(b => b.CustomerUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ================= Booking Reference =================

            builder.Property(b => b.BookingReference)
                .HasMaxLength(40)
                .IsRequired();

            builder.HasIndex(b => b.BookingReference)
                .IsUnique();

            // ================= Customer Details =================

            builder.Property(b => b.FullName)
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(b => b.Email)
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(b => b.MobileNumber)
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(b => b.CustomerNotes)
                .HasMaxLength(500);

            // ================= Guest Counts =================

            builder.Property(b => b.AdultCount)
                .IsRequired();

            builder.Property(b => b.ChildrenCount)
                .IsRequired();

            builder.Property(b => b.PersonCount)
                .IsRequired();

            // ================= Stay Dates =================

            builder.Property(b => b.CheckInDate)
                .HasColumnType("date")
                .IsRequired();

            builder.Property(b => b.CheckOutDate)
                .HasColumnType("date")
                .IsRequired();

            builder.Property(b => b.StayDays)
                .IsRequired();

            // ================= Booking Type =================

            builder.Property(b => b.BookingType)
                .HasMaxLength(30)
                .IsUnicode()
                .IsRequired();

            // ================= Pricing =================

            builder.Property(b => b.BaseAmount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(b => b.EventAmount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(b => b.TotalAmount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(b => b.AdvanceDailyRateSnapshot)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(b => b.AdvanceAmount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(b => b.BookingDailyRateSnapshot)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(b => b.EventDailyRateSnapshot)
                .HasColumnType("decimal(18,2)")
                .IsRequired(false);

            // ================= Booking Status =================

            builder.Property(b => b.Status)
                .HasMaxLength(30)
                .IsRequired();

            // ================= Approval Deadlines =================

            builder.Property(b => b.OwnerDeadlineUtc)
                .HasColumnType("datetime2(0)")
                .IsRequired();

            builder.Property(b => b.ApprovedAtUtc)
                .HasColumnType("datetime2(0)")
                .IsRequired(false);

            builder.Property(b => b.CustomerDeadlineUtc)
                .HasColumnType("datetime2(0)")
                .IsRequired(false);

            builder.Property(b => b.ExtensionUsed)
                .IsRequired();

            builder.Property(b => b.ExtensionReason)
                .HasMaxLength(1000)
                .IsRequired(false);

            // ================= Rejection / Cancellation =================

            builder.Property(b => b.RejectionReason)
                .HasMaxLength(1000)
                .IsRequired(false);

            builder.Property(b => b.CancellationReason)
                .HasMaxLength(1000)
                .IsRequired(false);

            // ================= Audit =================

            builder.Property(b => b.IsDelete)
                .IsRequired();

            builder.Property(b => b.CreatedBy)
                .IsRequired();

            builder.Property(b => b.CreatedDate)
                .HasColumnType("datetime2(0)")
                .IsRequired();

            builder.Property(b => b.ModifiedBy)
                .IsRequired(false);

            builder.Property(b => b.ModifiedDate)
                .HasColumnType("datetime2(0)")
                .IsRequired(false);

            // ================= Query Indexes =================

            builder.HasIndex(b => new
            {
                b.FarmHouseId,
                b.Status,
                b.CheckInDate,
                b.CheckOutDate
            });

            builder.HasIndex(b => new
            {
                b.FarmHouseId,
                b.CreatedDate
            });
        }
    }
}
