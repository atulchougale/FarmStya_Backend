
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FarmStay.Domain.Entities
{
    public class BookingReceipt
    {
        [Key]
        public long ReceiptId { get; set; }

        [Required]
        public int FarmHouseId { get; set; }

        [Required]
        public long BookingId { get; set; }

        [Required]
        public long PaymentTransactionId { get; set; }

        [Required]
        [MaxLength(50)]
        public string ReceiptNumber { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(3)]
        public string CurrencyCode { get; set; } = "INR";

        public DateTime IssuedAtUtc { get; set; } = DateTime.UtcNow;

        public long CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public long? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        // Navigation properties
        [ForeignKey(nameof(FarmHouseId))]
        public FarmHouse? FarmHouse { get; set; }

        [ForeignKey(nameof(BookingId))]
        public Booking? Booking { get; set; }

        [ForeignKey(nameof(PaymentTransactionId))]
        public PaymentTransaction? PaymentTransaction { get; set; }
    }
}
