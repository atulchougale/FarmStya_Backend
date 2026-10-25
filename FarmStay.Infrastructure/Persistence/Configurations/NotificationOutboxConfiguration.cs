
using FarmStay.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmStay.Infrastructure.Persistence.Configurations
{
    public class NotificationOutboxConfiguration
        : IEntityTypeConfiguration<NotificationOutbox>
    {
        public void Configure(
            EntityTypeBuilder<NotificationOutbox> builder)
        {
            builder.ToTable("NotificationOutbox");

            builder.HasKey(x => x.NotificationOutboxId);

            builder.Property(x => x.NotificationOutboxId)
                .UseIdentityColumn();

            builder.Property(x => x.Channel)
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(x => x.Recipient)
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(x => x.TemplateKey)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.PayloadJson)
                .HasColumnType("nvarchar(max)")
                .IsRequired();

            builder.Property(x => x.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Pending")
                .IsRequired();

            builder.Property(x => x.RetryCount)
                .HasDefaultValue(0);

            builder.Property(x => x.NextAttemptAtUtc)
                .HasColumnType("datetime2(0)");

            builder.Property(x => x.LastError)
                .HasMaxLength(1000);

            builder.Property(x => x.IdempotencyKey)
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(x => x.CreatedDate)
                .HasColumnType("datetime2(0)")
                .IsRequired();

            builder.Property(x => x.SentAtUtc)
                .HasColumnType("datetime2(0)");

            builder.HasOne(x => x.FarmHouse)
                .WithMany()
                .HasForeignKey(x => x.FarmHouseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Booking)
                .WithMany()
                .HasForeignKey(x => x.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.IdempotencyKey)
                .IsUnique();

            builder.HasIndex(x => new
            {
                x.Status,
                x.NextAttemptAtUtc,
                x.CreatedDate
            });
        }
    }
}
