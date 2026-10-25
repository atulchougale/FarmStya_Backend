
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FarmStay.Domain.Entities
{
    public class Booking
    {
        // ==================== Primary Key ====================

        [Key]
        public long BookingId { get; set; }

        // ==================== Tenant & Customer ====================

        [Required]
        public int FarmHouseId { get; set; }

        [Required]
        public int CustomerUserId { get; set; }

        [Required]
        [MaxLength(40)]
        public string BookingReference { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(30)]
        public string MobileNumber { get; set; } = string.Empty;

        // ==================== Guest Details ====================

        [Range(1, int.MaxValue)]
        public int AdultCount { get; set; }

        [Range(0, int.MaxValue)]
        public int ChildrenCount { get; set; }

        [Range(1, int.MaxValue)]
        public int PersonCount { get; set; }

        [MaxLength(500)]
        public string? CustomerNotes { get; set; }

        // ==================== Stay Details ====================

        [Required]
        public DateTime CheckInDate { get; set; }

        [Required]
        public DateTime CheckOutDate { get; set; }

        [Range(1, int.MaxValue)]
        public int StayDays { get; set; }

        // BookingOnly, NormalEvent, DecorationEvent
        [Required]
        [MaxLength(30)]
        public string BookingType { get; set; } = "BookingOnly";

        // ==================== Pricing Snapshots ====================

        public decimal BaseAmount { get; set; }

        public decimal EventAmount { get; set; }

        public decimal TotalAmount { get; set; }

        public decimal AdvanceDailyRateSnapshot { get; set; }

        public decimal AdvanceAmount { get; set; }

        public decimal BookingDailyRateSnapshot { get; set; }

        public decimal? EventDailyRateSnapshot { get; set; }

        // ==================== Booking Status ====================

        [Required]
        [MaxLength(30)]
        public string Status { get; set; } = "Pending";

        // ==================== Owner Approval Deadline ====================

        public DateTime OwnerDeadlineUtc { get; set; }

        public DateTime? ApprovedAtUtc { get; set; }

        // ==================== Customer Deadline ====================

        public DateTime? CustomerDeadlineUtc { get; set; }

        public bool ExtensionUsed { get; set; }

        [MaxLength(1000)]
        public string? ExtensionReason { get; set; }

        // ==================== Rejection & Cancellation ====================

        [MaxLength(1000)]
        public string? RejectionReason { get; set; }

        [MaxLength(1000)]
        public string? CancellationReason { get; set; }

        // ==================== Audit ====================

        public bool IsDelete { get; set; }

        public long CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public long? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }


        [ForeignKey(nameof(FarmHouseId))]
        public FarmHouse? FarmHouse { get; set; }

        [ForeignKey(nameof(CustomerUserId))]
        public User? CustomerUser { get; set; }
    }
}
