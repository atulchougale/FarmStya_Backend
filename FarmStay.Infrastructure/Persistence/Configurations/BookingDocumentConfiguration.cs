
using FarmStay.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmStay.Infrastructure.Persistence.Configurations
{
    public class BookingDocumentConfiguration
        : IEntityTypeConfiguration<BookingDocument>
    {
        public void Configure(EntityTypeBuilder<BookingDocument> builder)
        {
            // Table
            builder.ToTable("BookingDocuments");

            // Primary Key
            builder.HasKey(d => d.BookingDocumentId);

            builder.Property(d => d.BookingDocumentId)
                .ValueGeneratedOnAdd();

            // FarmHouse
            builder.Property(d => d.FarmHouseId)
                .IsRequired();

            builder.HasOne(d => d.FarmHouse)
                .WithMany()
                .HasForeignKey(d => d.FarmHouseId)
                .OnDelete(DeleteBehavior.Restrict);

            // Booking
            builder.Property(d => d.BookingId)
                .IsRequired();

            builder.HasOne(d => d.Booking)
                .WithMany()
                .HasForeignKey(d => d.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            // Uploader
            builder.Property(d => d.UserId)
                .IsRequired();

            builder.HasOne(d => d.User)
                .WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Document metadata
            builder.Property(d => d.DocumentType)
                .HasMaxLength(40)
                .IsUnicode()
                .IsRequired();

            builder.Property(d => d.StorageKey)
                .HasMaxLength(500)
                .IsUnicode()
                .IsRequired();

            builder.Property(d => d.OriginalFileName)
                .HasMaxLength(255)
                .IsUnicode()
                .IsRequired();

            builder.Property(d => d.ContentType)
                .HasMaxLength(100)
                .IsUnicode()
                .IsRequired();

            builder.Property(d => d.FileSizeBytes)
                .IsRequired();

            builder.Property(d => d.Sha256Hash)
                .HasColumnType("char(64)")
                .HasMaxLength(64)
                .IsUnicode(false)
                .IsRequired();

            // Verification
            builder.Property(d => d.VerificationStatus)
                .HasMaxLength(30)
                .IsUnicode()
                .HasDefaultValue("Uploaded")
                .IsRequired();

            builder.Property(d => d.VerificationReason)
                .HasMaxLength(1000)
                .IsUnicode()
                .IsRequired(false);

            builder.Property(d => d.VerifiedBy)
                .IsRequired(false);

            builder.Property(d => d.VerifiedAtUtc)
                .HasColumnType("datetime2(0)")
                .IsRequired(false);

            builder.Property(d => d.ReuploadDeadlineUtc)
                .HasColumnType("datetime2(0)")
                .IsRequired(false);

            // Soft Delete
            builder.Property(d => d.IsDelete)
                .HasDefaultValue(false)
                .IsRequired();

            // Audit
            builder.Property(d => d.CreatedBy)
                .IsRequired();

            builder.Property(d => d.CreatedDate)
                .HasColumnType("datetime2(0)")
                .IsRequired();

            builder.Property(d => d.ModifiedBy)
                .IsRequired(false);

            builder.Property(d => d.ModifiedDate)
                .HasColumnType("datetime2(0)")
                .IsRequired(false);

            // Query indexes
            builder.HasIndex(d => new
            {
                d.FarmHouseId,
                d.BookingId,
                d.VerificationStatus
            });

            builder.HasIndex(d => new
            {
                d.BookingId,
                d.CreatedDate
            });
        }
    }
}
