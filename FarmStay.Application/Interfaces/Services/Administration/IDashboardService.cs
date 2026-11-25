using FarmStay.Application.DTOs.Administration;
namespace FarmStay.Application.Interfaces.Services.Administration;
public interface IDashboardService
{
    Task<DashboardDto> GetAsync(CancellationToken ct);
    Task<IReadOnlyList<DashboardCalendarBookingDto>> CalendarAsync(int year, int month, CancellationToken ct);
}
