using FarmStay.Application.DTOs.Booking;
namespace FarmStay.Application.DTOs.Administration;
public sealed class DashboardSummaryDto
{
    public DateTime ServerToday { get; set; }
    public long BookingsThisWeek { get; set; }
    public long BookingsThisMonth { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal RevenueThisMonth { get; set; }
    public int OccupiedDays { get; set; }
    public int DaysInMonth { get; set; }
    public decimal OccupancyPercent { get; set; }
}
public sealed class DashboardCalendarBookingDto
{
    public DateTime StayDate { get; set; }
    public long BookingId { get; set; }
    public string BookingReference { get; set; } = "";
    public string FullName { get; set; } = "";
    public string Status { get; set; } = "";
}
public sealed class DashboardRevenueMonthDto
{
    public DateTime MonthStart { get; set; }
    public decimal Amount { get; set; }
}
public sealed class DashboardBookingSourceDto
{
    public string Source { get; set; } = "";
    public long BookingCount { get; set; }
}
public sealed record DashboardDto(DashboardSummaryDto Summary, IReadOnlyList<BookingListResponseDto> LatestBookings,
    IReadOnlyList<DashboardRevenueMonthDto> RevenueTrend, IReadOnlyList<DashboardBookingSourceDto> BookingSources);
