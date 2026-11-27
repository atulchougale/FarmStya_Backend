using System.Security.Claims;
using FarmStay.Application.Common.Constants;
using FarmStay.Application.DTOs.Administration;
using FarmStay.Application.Interfaces.Repositories;
using FarmStay.Application.Interfaces.Services.Administration;
using FarmStay.Application.Interfaces.Services.Booking;
using FarmStay.Application.Reporting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
namespace FarmStay.Application.Services.Administration;
public sealed class DashboardService(FarmStay.Application.Interfaces.Common.ICurrentTenantContext tenant, IDashboardRepository repository,
    IBookingListService bookings, IPermissionRepository permissions, ILogger<DashboardService> logger) : IDashboardService
{
    public async Task<DashboardDto> GetAsync(CancellationToken ct)
    {
        var farm = await Scope();
        var today = DateTime.Now.Date;
        var summary = await repository.SummaryAsync(farm, today, ct);
        var latest = await bookings.ListAsync(farm, null, 1, 5);
        var trend = await repository.RevenueTrendAsync(farm, today, ct);
        var sources = await repository.BookingSourcesAsync(farm, ct);
        return new(summary, latest, trend, sources);
    }
    public async Task<IReadOnlyList<DashboardCalendarBookingDto>> CalendarAsync(int year, int month, CancellationToken ct)
    {
        var farm = await Scope();
        if (year is < 1900 or > 9998 || month is < 1 or > 12)
            throw new ReportException(ReportError.InvalidRequest, "Choose a valid month and year.");
        var start = new DateTime(year, month, 1);
        return await repository.CalendarAsync(farm, start, start.AddMonths(1), ct);
    }
    private async Task<int> Scope()
    {
        var current = await tenant.RequireAsync();
        var user = current.UserId;
        var farm = current.FarmHouseId;
        var global = current.Role == "Super Admin";
        if (!await bookings.CanViewAsync(user, farm, true) || (!global && !await permissions.HasPermissionAsync(user, farm, "Dashboard.View")))
            throw new ReportException(ReportError.Forbidden, "You do not have access to this dashboard.");
        logger.LogInformation("Dashboard read by user {UserId} in farmhouse {Farmhouse}; correlation {CorrelationId}.", user, farm, current.CorrelationId);
        return farm;
    }
}
