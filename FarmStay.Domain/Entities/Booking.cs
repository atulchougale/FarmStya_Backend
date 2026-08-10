using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmStay.Domain.Entities
{
    public class Booking
    {
        // Primary Key
        public int BookingId { get; set; }

        // Foreign Keys
        public int UserId { get; set; }

        public int PropertyId { get; set; }

        // Booking Details
        public DateTime CheckInDate { get; set; }

        public DateTime CheckOutDate { get; set; }

        public decimal TotalAmount { get; set; }

        // Booking Status
        public string Status { get; set; } = "Pending";

        // Track status change date
        public DateTime? StatusChangedAt { get; set; }

        // Cancel / Reject reason
        public string? Remark { get; set; }

        // Soft Delete
        public bool IsDeleted { get; set; } = false;

        // Record creation time
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public User? User { get; set; }

        public Property? Property { get; set; }

        // One Booking → Many Logs
        public ICollection<BookingLog>? BookingLogs { get; set; }
    }
}