
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FarmStay.Domain.Entities
{
    public class BookingStatusHistory
    {
        [Key]
        public long BookingStatusHistoryId { get; set; }

        [Required]
        public int FarmHouseId { get; set; }

        [Required]
        public long BookingId { get; set; }

        [MaxLength(30)]
        public string? OldStatus { get; set; }

        [Required]
        [MaxLength(30)]
        public string NewStatus { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Reason { get; set; }

        // Null when the action is performed by a system worker.
        public int? ActorUserId { get; set; }

        [Required]
        [MaxLength(20)]
        public string ActorType { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey(nameof(FarmHouseId))]
        public FarmHouse? FarmHouse { get; set; }

        [ForeignKey(nameof(BookingId))]
        public Booking? Booking { get; set; }

        [ForeignKey(nameof(ActorUserId))]
        public User? ActorUser { get; set; }
    }
}
