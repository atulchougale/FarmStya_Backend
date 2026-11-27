namespace FarmStay.Application.DTOs.Booking
{
    public class ActiveBookingDateReservationDto
    {
        public int FarmHouseId { get; set; }

        public Guid SelectionId { get; set; }
        public List<long> ReservationIds { get; set; } = new();

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public List<DateTime> ReservedDates { get; set; } = new();

        public DateTime ExpiresAt { get; set; }
    }
}
