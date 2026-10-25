
using FarmStay.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmStay.Infrastructure.Persistence.Configurations
{
    public class BookingEventDateConfiguration
        : IEntityTypeConfiguration<BookingEventDate>
    {
        public void Configure(EntityTypeBuilder<BookingEventDate> builder)
        {
            // Table
            builder.ToTable("BookingEventDates");

            // Primary Key
            builder.HasKey(e => e.BookingEventDateId);

            builder.Property(e => e.BookingEventDateId)
                .ValueGeneratedOnAdd();

            // Tenant
            builder.Property(e => e.FarmHouseId)
                .IsRequired();

            builder.HasOne(e => e.FarmHouse)
                .WithMany()
                .HasForeignKey(e => e.FarmHouseId)
                .OnDelete(DeleteBehavior.Restrict);

            // Booking
            builder.Property(e => e.BookingId)
                .IsRequired();

            builder.HasOne(e => e.Booking)
                .WithMany()
                .HasForeignKey(e => e.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            // Event Date
            builder.Property(e => e.EventDate)
                .HasColumnType("date")
                .IsRequired();

            // Event Type
            builder.Property(e => e.EventType)
                .HasMaxLength(30)
                .IsUnicode()
                .IsRequired();

            // Rate Snapshot
            builder.Property(e => e.DailyRateSnapshot)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            // Soft Delete
            builder.Property(e => e.IsDelete)
                .HasDefaultValue(false)
                .IsRequired();

            // Audit
            builder.Property(e => e.CreatedBy)
                .IsRequired();

            builder.Property(e => e.CreatedDate)
                .HasColumnType("datetime2(0)")
                .IsRequired();

            builder.Property(e => e.ModifiedBy)
                .IsRequired(false);

            builder.Property(e => e.ModifiedDate)
                .HasColumnType("datetime2(0)")
                .IsRequired(false);

            // Query index
            builder.HasIndex(e => new
            {
                e.FarmHouseId,
                e.BookingId,
                e.EventDate
            });

            // Prevent duplicate active event entries for the same
            // booking and date.
            builder.HasIndex(e => new
            {
                e.BookingId,
                e.EventDate
            })
            .IsUnique()
            .HasFilter("[IsDelete] = 0");
        }
    }
}
