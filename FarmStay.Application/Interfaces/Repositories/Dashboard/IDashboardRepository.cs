using FarmStay.Application.DTOs.Administration;
namespace FarmStay.Application.Interfaces.Repositories;
public interface IDashboardRepository
{
    Task<DashboardSummaryDto> SummaryAsync(int farmhouse, DateTime today, CancellationToken ct);
    Task<IReadOnlyList<DashboardCalendarBookingDto>> CalendarAsync(int farmhouse, DateTime from, DateTime to, CancellationToken ct);
    Task<IReadOnlyList<DashboardRevenueMonthDto>> RevenueTrendAsync(int farmhouse, DateTime today, CancellationToken ct);
    Task<IReadOnlyList<DashboardBookingSourceDto>> BookingSourcesAsync(int farmhouse, CancellationToken ct);
}
