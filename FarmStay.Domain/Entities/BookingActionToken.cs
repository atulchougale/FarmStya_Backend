
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FarmStay.Domain.Entities
{
    public class BookingActionToken
    {
        [Key]
        public long BookingActionTokenId { get; set; }

        [Required]
        public int FarmHouseId { get; set; }

        [Required]
        public long BookingId { get; set; }

        [Required]
        [MaxLength(64)]
        public string TokenHash { get; set; } = string.Empty;

        [Required]
        [MaxLength(30)]
        public string Purpose { get; set; } = string.Empty;

        public DateTime ExpiresAtUtc { get; set; }

        public DateTime? ConsumedAtUtc { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public bool IsDelete { get; set; } = false;

        public long CreatedBy { get; set; }

        public long? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        // Navigation properties
        [ForeignKey(nameof(FarmHouseId))]
        public FarmHouse? FarmHouse { get; set; }

        [ForeignKey(nameof(BookingId))]
        public Booking? Booking { get; set; }
    }
}
