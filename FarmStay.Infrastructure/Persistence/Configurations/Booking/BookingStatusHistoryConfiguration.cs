
using FarmStay.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmStay.Infrastructure.Persistence.Configurations
{
    public class BookingStatusHistoryConfiguration
        : IEntityTypeConfiguration<BookingStatusHistory>
    {
        public void Configure(
            EntityTypeBuilder<BookingStatusHistory> builder)
        {
            builder.ToTable("BookingStatusHistory");

            builder.HasKey(x => x.BookingStatusHistoryId);

            builder.Property(x => x.BookingStatusHistoryId)
                .UseIdentityColumn();

            builder.Property(x => x.OldStatus)
                .HasMaxLength(30);

            builder.Property(x => x.NewStatus)
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(x => x.Reason)
                .HasMaxLength(1000);

            builder.Property(x => x.ActorType)
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(x => x.CreatedDate)
                .HasColumnType("datetime2(0)")
                .IsRequired();

            builder.HasOne(x => x.FarmHouse)
                .WithMany()
                .HasForeignKey(x => x.FarmHouseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Booking)
                .WithMany()
                .HasForeignKey(x => x.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ActorUser)
                .WithMany()
                .HasForeignKey(x => x.ActorUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new
            {
                x.FarmHouseId,
                x.BookingId,
                x.CreatedDate
            });

            builder.HasIndex(x => new
            {
                x.BookingId,
                x.NewStatus
            });
        }
    }
}
