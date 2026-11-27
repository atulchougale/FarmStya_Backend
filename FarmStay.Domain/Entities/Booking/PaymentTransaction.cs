
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FarmStay.Domain.Entities
{
    public class PaymentTransaction
    {
        [Key]
        public long PaymentTransactionId { get; set; }

        [Required]
        public int FarmHouseId { get; set; }

        [Required]
        public long BookingId { get; set; }

        [Required]
        [MaxLength(30)]
        public string PaymentType { get; set; } = string.Empty;
        // Advance / Checkout / Refund

        [Required]
        [MaxLength(30)]
        public string PaymentMethod { get; set; } = string.Empty;
        // Online / Cash

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(3)]
        public string CurrencyCode { get; set; } = "INR";

        [Required]
        [MaxLength(30)]
        public string PaymentStatus { get; set; } = "Pending";
        // Pending / Success / Failed / Cancelled / Refunded

        [MaxLength(50)]
        public string? PaymentProvider { get; set; }

        [MaxLength(150)]
        public string? ProviderOrderId { get; set; }

        [MaxLength(150)]
        public string? ProviderPaymentId { get; set; }

        [MaxLength(150)]
        public string? ProviderRefundId { get; set; }

        [MaxLength(150)]
        public string IdempotencyKey { get; set; } = string.Empty;

        public long? OriginalPaymentTransactionId { get; set; }
        public long? MoneyAccountId { get; set; }
        [MaxLength(20)] public string? RefundAccountingKind { get; set; }
        [MaxLength(1000)] public string? RefundReason { get; set; }
        [MaxLength(150)] public string? RefundReference { get; set; }
        [MaxLength(64)] public string? RefundRequestHash { get; set; }
        public DateTime? PaidAt { get; set; }

        public DateTime? VerifiedAt { get; set; }

        [MaxLength(1000)]
        public string? FailureReason { get; set; }

        public bool IsDelete { get; set; } = false;

        public long CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public long? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        // Navigation
        [ForeignKey(nameof(FarmHouseId))]
        public FarmHouse? FarmHouse { get; set; }

        [ForeignKey(nameof(BookingId))]
        public Booking? Booking { get; set; }
    }
}
