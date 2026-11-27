using System.Diagnostics;
using FarmStay.Application.DTOs.Administration;
using FarmStay.Application.Interfaces.Repositories;
using FarmStay.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
namespace FarmStay.Infrastructure.Repositories.Administration;
public sealed class DashboardRepository(AppDbContext db, ILogger<DashboardRepository> logger) : IDashboardRepository
{
    public async Task<DashboardSummaryDto> SummaryAsync(int farmhouse, DateTime today, CancellationToken ct) =>
        (await Read<DashboardSummaryDto>($"EXEC dbo.usp_Dashboard_GetSummary @FarmHouseId={farmhouse}, @Today={today}", "usp_Dashboard_GetSummary", ct)).Single();
    public Task<IReadOnlyList<DashboardCalendarBookingDto>> CalendarAsync(int farmhouse, DateTime from, DateTime to, CancellationToken ct) =>
        Read<DashboardCalendarBookingDto>($"EXEC dbo.usp_Dashboard_GetCalendar @FarmHouseId={farmhouse}, @FromDate={from}, @ToDateExclusive={to}", "usp_Dashboard_GetCalendar", ct);
    public Task<IReadOnlyList<DashboardRevenueMonthDto>> RevenueTrendAsync(int farmhouse, DateTime today, CancellationToken ct) =>
        Read<DashboardRevenueMonthDto>($"EXEC dbo.usp_Dashboard_GetRevenueTrend @FarmHouseId={farmhouse}, @Today={today}", "usp_Dashboard_GetRevenueTrend", ct);
    public Task<IReadOnlyList<DashboardBookingSourceDto>> BookingSourcesAsync(int farmhouse, CancellationToken ct) =>
        Read<DashboardBookingSourceDto>($"EXEC dbo.usp_Dashboard_GetBookingSources @FarmHouseId={farmhouse}", "usp_Dashboard_GetBookingSources", ct);
    private async Task<IReadOnlyList<T>> Read<T>(FormattableString query, string procedure, CancellationToken ct) where T : class
    {
        var timer = Stopwatch.StartNew();var timeout = db.Database.GetCommandTimeout();
        try { db.Database.SetCommandTimeout(60);var data = await db.Set<T>().FromSqlInterpolated(query).AsNoTracking().ToListAsync(ct);
            logger.LogInformation("Dashboard SP {Procedure} returned {Rows} rows in {Elapsed} ms.", procedure, data.Count, timer.ElapsedMilliseconds);return data; }
        catch(Exception e) when(e is not OperationCanceledException) { logger.LogError(e,"Dashboard SP {Procedure} failed.",procedure);throw new InvalidOperationException("Dashboard data could not be loaded. Please retry.",e); }
        finally { db.Database.SetCommandTimeout(timeout); }
    }
}
