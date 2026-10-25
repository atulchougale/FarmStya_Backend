
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FarmStay.Domain.Entities
{
    public class BookingDateReservation
    {
        [Key]
        public long ReservationId { get; set; }

        [Required]
        public int FarmHouseId { get; set; }

        [Required]
        public int UserId { get; set; }

        // Groups dates selected in one booking form session.
        public Guid SelectionId { get; set; }

        [Required]
        public DateTime ReservationDate { get; set; }

        [Required]
        public DateTime ExpiresAtUtc { get; set; }

        public DateTime CreatedDateUtc { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey(nameof(FarmHouseId))]
        public FarmHouse? FarmHouse { get; set; }

        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }
    }
}
