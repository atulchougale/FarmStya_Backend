
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FarmStay.Domain.Entities
{
    public class NotificationOutbox
    {
        [Key]
        public long NotificationOutboxId { get; set; }

        public int? FarmHouseId { get; set; }

        public long? BookingId { get; set; }

        [Required]
        [MaxLength(20)]
        public string Channel { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string Recipient { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string TemplateKey { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "nvarchar(max)")]
        public string PayloadJson { get; set; } = "{}";

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Pending";

        public int RetryCount { get; set; } = 0;

        public DateTime? NextAttemptAtUtc { get; set; }

        [MaxLength(1000)]
        public string? LastError { get; set; }

        [Required]
        [MaxLength(150)]
        public string IdempotencyKey { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? SentAtUtc { get; set; }

        // Navigation properties
        [ForeignKey(nameof(FarmHouseId))]
        public FarmHouse? FarmHouse { get; set; }

        [ForeignKey(nameof(BookingId))]
        public Booking? Booking { get; set; }
    }
}
