
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FarmStay.Domain.Entities
{
    public class CheckoutDiscount
    {
        [Key]
        public long CheckoutDiscountId { get; set; }

        [Required]
        public int FarmHouseId { get; set; }

        [Required]
        public long BookingId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Reason { get; set; } = string.Empty;

        [Required]
        public long EnteredBy { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public bool IsDelete { get; set; } = false;

        // Navigation properties
        [ForeignKey(nameof(FarmHouseId))]
        public FarmHouse? FarmHouse { get; set; }

        [ForeignKey(nameof(BookingId))]
        public Booking? Booking { get; set; }
    }
}
