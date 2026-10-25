
namespace FarmStay.Application.DTOs.Booking
{
    public class BookingCalendarMonthResponseDto
    {
        public int FarmHouseId { get; set; }

        public int Month { get; set; }

        public int Year { get; set; }

        public List<CalendarBookedDateDto> BookedDates { get; set; } = new();

        public List<CalendarReservedDateDto> ReservedDates { get; set; } = new();
    }

    public class CalendarBookedDateDto
    {
        public DateTime Date { get; set; }

        public string Status { get; set; } = string.Empty;
    }

    public class CalendarReservedDateDto
    {
        public DateTime Date { get; set; }

        public string Status { get; set; } = "TemporarilyReserved";

        public DateTime ExpiresAtUtc { get; set; }
    }
}
