using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmStay.Domain.Entities
{
    public class BookingLog
    {
        // Primary Key
        public int BookingLogId { get; set; }

        // Foreign Keys
        public int BookingId { get; set; }

        public int UserId { get; set; }

        /*
            Action Types:
            B = Booking Created
            A = Approved
            R = Rejected
            C = Cancelled
            D = Deleted
        */
        public string ActionType { get; set; } = string.Empty;

        // Optional Remark
        public string? Remark { get; set; }

        // Action Date & Time
        public DateTime ActionDate { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public Booking? Booking { get; set; }

        public User? User { get; set; }

    }
}
