
namespace FarmStay.Application.DTOs.Booking
{

    public class CreateBookingDateReservationRequestDto
    {
        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }
    }
    public class CreateBookingDateReservationsDto
    {
        public int FarmHouseId { get; set; }

        public Guid SelectionId { get; set; }
        public List<long> ReservationIds { get; set; } = new();

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public List<DateTime> ReservedDates { get; set; } = new();

        public DateTime ExpiresAt { get; set; }
    }


    public class DeleteBookingDateReservationRequestDto
    {
        // Start date (or the single date when ToDate is null).
        public DateTime FromDate { get; set; }

        // Optional. When provided, the range is [FromDate, ToDate) - ToDate is exclusive.
        public DateTime? ToDate { get; set; }
    }

    public class DeleteBookingDateReservationResponseDto
    {
        public int FarmHouseId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int DeletedCount { get; set; }
        public List<DateTime> ReleasedDates { get; set; } = new();
    }
}
