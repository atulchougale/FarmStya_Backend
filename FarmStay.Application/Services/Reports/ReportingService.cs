using System.Security.Claims;
using FarmStay.Application.Common.Constants;
using FarmStay.Application.Interfaces.Repositories;
using FarmStay.Application.Interfaces.Services.Booking;
using FarmStay.Application.Interfaces.Services.Reports;
using FarmStay.Application.Reporting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace FarmStay.Application.Services.Reports;
public sealed class ReportingService(IHttpContextAccessor http, IUserMembershipRepository memberships,
    IFarmHouseRepository farmhouses, IBookingListService administration, IReportAccess access,
    IBookingReportService booking, IRevenueReportService revenue, ICustomerReportService customer,
    IOccupancyReportService occupancy, ILogger<ReportingService> logger) : IReportingService
{
    private static readonly SemaphoreSlim ExportSlots = new(2);
    private ReportScope? resolved;
    public async Task<IReadOnlyList<ReportCatalogEntry>> CatalogAsync(CancellationToken ct)
    {
        var scope = await ResolveAsync(ct);
        return ReportCatalog.All.Where(d => scope.GlobalAdministrator || scope.Permissions.Contains(d.Permission))
            .Select(d => new ReportCatalogEntry(d.Key, d.Title, d.Description, d.Columns,
                scope.GlobalAdministrator || scope.Permissions.Contains(d.Permission.Replace(".View", ".Export")))).ToArray();
    }
    public async Task<IReportPage> ListAsync(string report, ReportFilter filter, CancellationToken ct)
    {
        var scope = await ResolveAsync(ct);
        using var audit = Audit(scope, report);
        return report switch {
            "bookings" => await booking.ListAsync(scope, filter, ct),
            "revenue" => await revenue.ListAsync(scope, filter, ct),
            "customers" => await customer.ListAsync(scope, filter, ct),
            "occupancy" => await occupancy.ListAsync(scope, filter, ct),
            _ => throw new ReportException(ReportError.NotFound, "Unknown report.")
        };
    }
    public async Task<ReportDownload> ExportAsync(string report, ReportFilter filter, string format, CancellationToken ct)
    {
        var scope = await ResolveAsync(ct);
        using var audit = Audit(scope, report);
        await ExportSlots.WaitAsync(ct);
        try
        {
            var file = report switch {
                "bookings" => await booking.ExportAsync(scope, filter, format, ct),
                "revenue" => await revenue.ExportAsync(scope, filter, format, ct),
                "customers" => await customer.ExportAsync(scope, filter, format, ct),
                "occupancy" => await occupancy.ExportAsync(scope, filter, format, ct),
                _ => throw new ReportException(ReportError.NotFound, "Unknown report.")
            };
            logger.LogInformation("Report {Report} exported as {Format} by user {UserId} for farmhouse {FarmHouseId}; correlation {CorrelationId}.",
                report, format, scope.UserId, scope.FarmHouseId, scope.CorrelationId);
            return file;
        }
        finally { ExportSlots.Release(); }
    }
    private IDisposable? Audit(ReportScope scope, string report) => logger.BeginScope(new Dictionary<string, object> {
        ["Report"] = report, ["UserId"] = scope.UserId, ["FarmHouseId"] = scope.FarmHouseId, ["CorrelationId"] = scope.CorrelationId });

    private async Task<ReportScope> ResolveAsync(CancellationToken ct)
    {
        if (resolved != null) return resolved;
        ct.ThrowIfCancellationRequested();
        var context = http.HttpContext;
        if (context?.User.Identity?.IsAuthenticated != true ||
            !int.TryParse(context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var user) || user <= 0)
            throw new ReportException(ReportError.Unauthorized, "Authentication is required.");
        if (!int.TryParse(context.Request.Headers["FarmHouseId"].FirstOrDefault(), out var farmhouse) || farmhouse <= 0)
            throw new ReportException(ReportError.InvalidRequest, "A valid farmhouse is required.");
        var farm = await farmhouses.GetByIdAsync(farmhouse);
        if (farm is not { IsActive: true, IsDeleted: false }) throw new ReportException(ReportError.NotFound, "Farmhouse not found.");
        var global = await administration.IsGlobalAdministratorAsync(user);
        if (!global)
        {
            if (context.User.FindFirst(JwtClaimNames.FarmHouseId)?.Value != farmhouse.ToString())
                throw new ReportException(ReportError.Forbidden, "You do not have access to this farmhouse.");
            var member = await memberships.GetMembershipAsync(user, farmhouse);
            if (member?.User is not { IsActive: true, IsDeleted: false } ||
                member.Role is not { IsActive: true, IsDeleted: false, RoleName: "Admin" or "Farm Owner" or "FarmOwner" })
                throw new ReportException(ReportError.Forbidden, "An active administration membership is required.");
        }
        IReadOnlySet<string> rights = global ? new HashSet<string>() : await access.PermissionsAsync(farmhouse, user, ct);
        return resolved = new(farmhouse, user, farm.FarmHouseName, farm.Address, farm.MobileNumber,
            global, rights, context.TraceIdentifier);
    }
}
