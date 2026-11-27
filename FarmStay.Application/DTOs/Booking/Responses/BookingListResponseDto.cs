using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace FarmStay.Application.DTOs.Booking
{
    public class BookingListResponseDto
    {
        public long BookingId { get; set; }
        public string BookingSource { get; set; } = "Online";
        public long CreatedBy { get; set; }
        public bool CanManageConfirmation { get; set; }
        public bool HasPendingAdvanceOrder { get; set; }
        public string? BookingReference { get; set; }
        public string? FullName { get; set; }
        public string? MobileNumber { get; set; }
        public string? Email { get; set; }
        public string BookingType { get; set; } = string.Empty;
        public int StayDays { get; set; }
        public decimal BaseAmount { get; set; }
        public decimal EventAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AdvanceAmount { get; set; }
        public decimal AdvanceDailyRateSnapshot { get; set; }
        public decimal BookingDailyRateSnapshot { get; set; }
        public decimal? EventDailyRateSnapshot { get; set; }
        public string CurrencyCode { get; set; } = "INR";
        public string? CustomerNotes { get; set; }
        public string? RejectionReason { get; set; }
        public string? CancellationReason { get; set; }
        public DateTime OwnerDeadline { get; set; }
        public DateTime? CustomerDeadline { get; set; }
        public bool CanDecide { get; set; }
        public bool CanCheckIn { get; set; }
        public bool CanCheckout { get; set; }
        public decimal RefundedAmount { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal AdvancePaid { get; set; }
        public decimal DiscountAmount { get; set; }
        public List<BookingDiscountDetails> Discounts { get; set; } = [];
        public sealed record BookingDiscountDetails(decimal Amount, string Reason, long EnteredBy, string? EnteredByName, DateTime CreatedDate);
        public decimal RemainingAmount { get; set; }
        public bool HasIdentityDocument { get; set; }
        public string? IdentityDocumentType { get; set; }
        public string? OtherIdentityDocumentType { get; set; }
        public int IdentityFileCount { get; set; }



        [Range(1, int.MaxValue)]
        public int AdultCount { get; set; }

        [Range(0, int.MaxValue)]
        public int ChildrenCount { get; set; }

        [Range(1, int.MaxValue)]
        public int PersonCount { get; set; }


        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public DateTime? ActualCheckInAt { get; set; }
        public DateTime? ActualCheckOutAt { get; set; }
        public DateTime? CheckoutPaidAt { get; set; }





        // Current Booking Status
        public string? Status { get; set; }
        public DateTime CreatedDate { get; set; }

        // Event Date Details
        public List<EventDateDetails> EventDates { get; set; } = new();

        public List<StatusHistoryDetails> StatusHistory { get; set; } = new();


        public class EventDateDetails
        {
            public long BookingEventDateId { get; set; }

            public DateTime EventDate { get; set; }

            public string EventType { get; set; } = string.Empty;

            public decimal DailyRateSnapshot { get; set; }
        }

        // Status History DTO inside the same file
        public class StatusHistoryDetails
        {
            public string? OldStatus { get; set; }

            public string NewStatus { get; set; } = string.Empty;

            public string? Reason { get; set; }

            public string ActorType { get; set; } = string.Empty;

            public DateTime CreatedDate { get; set; }
        }
    }






}
