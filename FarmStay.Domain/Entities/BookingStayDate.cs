
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FarmStay.Domain.Entities
{
    public class BookingStayDate
    {
        [Key]
        public long BookingStayDateId { get; set; }

        [Required]
        public int FarmHouseId { get; set; }

        [Required]
        public long BookingId { get; set; }

        [Required]
        public DateTime StayDate { get; set; }

        public long CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public long? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public bool IsDelete { get; set; } = false;

        // Navigation properties
        [ForeignKey(nameof(FarmHouseId))]
        public FarmHouse? FarmHouse { get; set; }

        [ForeignKey(nameof(BookingId))]
        public Booking? Booking { get; set; }
    }
}
