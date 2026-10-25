
namespace FarmStay.Application.DTOs.Booking
{
    public class CreateBookingDateReservationResponseDto
    {
        public int FarmHouseId { get; set; }

        public Guid SelectionId { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public List<DateTime> ReservedDates { get; set; } = new();

        public DateTime ExpiresAtUtc { get; set; }
    }
}
