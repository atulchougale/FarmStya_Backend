
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FarmStay.Domain.Entities
{
    public class BookingEventDate
    {
        [Key]
        public long BookingEventDateId { get; set; }

        [Required]
        public int FarmHouseId { get; set; }

        [Required]
        public long BookingId { get; set; }

        [Required]
        public DateTime EventDate { get; set; }

        [Required]
        [MaxLength(30)]
        public string EventType { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal DailyRateSnapshot { get; set; }

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
    }
}
