
namespace FarmStay.Application.DTOs.Booking
{
    public class BookingCalendarAvailabilityResponseDto
    {
        public int FarmHouseId { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public bool IsAvailable { get; set; }

        public List<DateTime> AvailableDates { get; set; } = new();

        public List<DateTime> UnavailableDates { get; set; } = new();

        public List<CalendarPermanentDateDto> PermanentDates { get; set; } = new();

        public List<CalendarActiveHoldDto> ActiveHolds { get; set; } = new();
    }

    public class CalendarPermanentDateDto
    {
        public DateTime Date { get; set; }

        public string Status { get; set; } = "Unavailable";
    }

    public class CalendarActiveHoldDto
    {
        public DateTime Date { get; set; }

        public string Status { get; set; } = "TemporarilyReserved";

        public DateTime ExpiresAtUtc { get; set; }
    }
}
