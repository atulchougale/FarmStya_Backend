
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FarmStay.Domain.Entities
{
    public class BookingDocument
    {
        [Key]
        public long BookingDocumentId { get; set; }

        [Required]
        public int FarmHouseId { get; set; }

        [Required]
        public long BookingId { get; set; }

        // Existing User entity uses int UserId.
        [Required]
        public int UserId { get; set; }

        [Required]
        [MaxLength(40)]
        public string DocumentType { get; set; } = string.Empty;

        // Private storage reference, not a public URL.
        [Required]
        [MaxLength(500)]
        public string StorageKey { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string OriginalFileName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string ContentType { get; set; } = string.Empty;

        public long FileSizeBytes { get; set; }

        [Required]
        [Column(TypeName = "char(64)")]
        [MaxLength(64)]
        public string Sha256Hash { get; set; } = string.Empty;

        [Required]
        [MaxLength(30)]
        public string VerificationStatus { get; set; } = "Uploaded";

        [MaxLength(1000)]
        public string? VerificationReason { get; set; }

        public long? VerifiedBy { get; set; }

        public DateTime? VerifiedAtUtc { get; set; }

        public DateTime? ReuploadDeadlineUtc { get; set; }

        public bool IsDelete { get; set; } = false;

        public long CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public long? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        // Navigation properties
        [ForeignKey(nameof(FarmHouseId))]
        public FarmHouse? FarmHouse { get; set; }

        [ForeignKey(nameof(BookingId))]
        public Booking? Booking { get; set; }

        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }
    }
}
