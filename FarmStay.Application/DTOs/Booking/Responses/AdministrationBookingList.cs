namespace FarmStay.Application.DTOs.Booking;
public sealed class AdministrationBookingFilter
{
 public int Page { get; set; } = 1;
 public int PageSize { get; set; } = 5;
 public string? Search { get; set; }
 public string? Status { get; set; }
 public string? Source { get; set; }
 public DateTime? From { get; set; }
 public DateTime? To { get; set; }
}
public sealed record AdministrationBookingSummary(long Total, long Confirmed, long CheckedIn, long CheckedOut, long Pending);
public sealed record AdministrationBookingList(IReadOnlyList<BookingListResponseDto> Items, long TotalCount, AdministrationBookingSummary Summary);
