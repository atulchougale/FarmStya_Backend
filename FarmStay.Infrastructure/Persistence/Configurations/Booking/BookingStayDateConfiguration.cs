
using FarmStay.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmStay.Infrastructure.Persistence.Configurations
{
    public class BookingStayDateConfiguration
        : IEntityTypeConfiguration<BookingStayDate>
    {
        public void Configure(EntityTypeBuilder<BookingStayDate> builder)
        {
            // Table
            builder.ToTable("BookingStayDates");

            // Primary Key
            builder.HasKey(s => s.BookingStayDateId);

            builder.Property(s => s.BookingStayDateId)
                .ValueGeneratedOnAdd();

            // FarmHouse
            builder.Property(s => s.FarmHouseId)
                .IsRequired();

            builder.HasOne(s => s.FarmHouse)
                .WithMany()
                .HasForeignKey(s => s.FarmHouseId)
                .OnDelete(DeleteBehavior.Restrict);

            // Booking
            builder.Property(s => s.BookingId)
                .IsRequired();

            builder.HasOne(s => s.Booking)
                .WithMany()
                .HasForeignKey(s => s.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            // Occupied Stay Date
            builder.Property(s => s.StayDate)
                .HasColumnType("date")
                .IsRequired();

            // Audit
            builder.Property(s => s.CreatedBy)
                .IsRequired();

            builder.Property(s => s.CreatedDate)
                .HasColumnType("datetime2(0)")
                .IsRequired();

            builder.Property(s => s.ModifiedBy)
                .IsRequired(false);

            builder.Property(s => s.ModifiedDate)
                .HasColumnType("datetime2(0)")
                .IsRequired(false);

            // Soft Delete
            builder.Property(s => s.IsDelete)
                .HasDefaultValue(false)
                .IsRequired();

            // One active booking per farmhouse per date
            builder.HasIndex(s => new
            {
                s.FarmHouseId,
                s.StayDate
            })
            .IsUnique()
            .HasFilter("[IsDelete] = 0");

            // Find all dates belonging to a booking
            builder.HasIndex(s => new
            {
                s.BookingId,
                s.StayDate
            });
        }
    }
}
