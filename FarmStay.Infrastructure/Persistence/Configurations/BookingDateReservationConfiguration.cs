
using FarmStay.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmStay.Infrastructure.Persistence.Configurations
{
    public class BookingDateReservationConfiguration
        : IEntityTypeConfiguration<BookingDateReservation>
    {
        public void Configure(
            EntityTypeBuilder<BookingDateReservation> builder)
        {
            // Table
            builder.ToTable("BookingDateReservations");

            // Primary Key
            builder.HasKey(r => r.ReservationId);

            builder.Property(r => r.ReservationId)
                .ValueGeneratedOnAdd();

            // FarmHouse
            builder.Property(r => r.FarmHouseId)
                .IsRequired();

            builder.HasOne(r => r.FarmHouse)
                .WithMany()
                .HasForeignKey(r => r.FarmHouseId)
                .OnDelete(DeleteBehavior.Restrict);

            // User
            builder.Property(r => r.UserId)
                .IsRequired();

            builder.HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Selection
            builder.Property(r => r.SelectionId)
                .IsRequired();

            // Reserved Date
            builder.Property(r => r.ReservationDate)
                .HasColumnType("date")
                .IsRequired();

            // Hold Expiry
            builder.Property(r => r.ExpiresAtUtc)
                .HasColumnType("datetime2(0)")
                .IsRequired();

            // Creation Timestamp
            builder.Property(r => r.CreatedDateUtc)
                .HasColumnType("datetime2(0)")
                .IsRequired();

            // Efficient calendar/availability queries
            builder.HasIndex(r => new
            {
                r.FarmHouseId,
                r.ReservationDate,
                r.ExpiresAtUtc
            });

            // Retrieve all dates in a selection
            builder.HasIndex(r => new
            {
                r.SelectionId,
                r.UserId
            });
        }
    }
}
