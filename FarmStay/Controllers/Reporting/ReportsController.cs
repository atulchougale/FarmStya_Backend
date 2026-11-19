using System.Security.Claims;
using FarmStay.Application.Common.Constants;
using FarmStay.Application.Interfaces.Repositories;
using FarmStay.Application.Interfaces.Services.Booking;
using FarmStay.Application.Reporting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmStay.API.Controllers.Reporting;
[ApiController]
[Route("api/administration/reports")]
[Authorize(Roles = "Admin,Farm Owner,FarmOwner,Super Admin")]
public sealed class ReportsController(IReportingService reporting, IBookingListService administration,
    IUserMembershipRepository memberships, IFarmHouseRepository farmhouses, IReportAccess access,
    ILogger<ReportsController> logger) : ControllerBase
{
    private bool globalAdministrator;
    private IReadOnlySet<string>? granted;
    [HttpGet]
    public async Task<IActionResult> Catalog()
    {
        var (error, farm, user) = await Scope();
        if (error != null) return error;
        var available = new List<object>();
        foreach (var report in ReportCatalog.All)
            if (await Allowed(farm, user, report.Permission)) available.Add(new { report.Key, report.Title, report.Description,
                report.Columns, CanExport = await Allowed(farm, user, report.Permission.Replace(".View", ".Export")) });
        return Ok(new { success = true, data = available });
    }
    [HttpGet("{report}")]
    public async Task<IActionResult> List(string report, [FromQuery] ReportFilter filter, CancellationToken ct) =>
        await Execute(report, filter, null, ct);
    [HttpGet("{report}/export/{format}")]
    public async Task<IActionResult> Export(string report, string format, [FromQuery] ReportFilter filter, CancellationToken ct) =>
        await Execute(report, filter, format, ct);

    private async Task<IActionResult> Execute(string key, ReportFilter filter, string? format, CancellationToken ct)
    {
        try
        {
            var definition = ReportCatalog.Find(key);
            filter.Resolve();
            if (format != null && format is not ("pdf" or "excel")) return BadRequest(new { message = "Choose PDF or Excel." });
            var (error, farm, user) = await Scope();
            if (error != null) return error;
            if (!await Allowed(farm, user, definition.Permission) ||
                (format != null && !await Allowed(farm, user, definition.Permission.Replace(".View", ".Export")))) return Forbid();
            Response.Headers.CacheControl = "no-store";
            if (format == null) return Ok(new { success = true, data = await reporting.ListAsync(farm, key, filter, ct) });
            var file = await reporting.ExportAsync(farm, key, filter, format, ct);
            logger.LogInformation("Report {Report} exported as {Format} by user {UserId} in farmhouse {Farmhouse}.", key, format, user, farm);
            return File(file.Content, file.ContentType, file.FileName);
        }
        catch (ArgumentException e) { return BadRequest(new { message = e.Message }); }
        catch (InvalidOperationException e) when (e.Message.StartsWith("Export limit"))
        { return StatusCode(413, new { message = e.Message }); }
    }
    private async Task<bool> Allowed(int farm, int user, string permission)
    {
        if (globalAdministrator) return true;
        granted ??= await access.PermissionsAsync(farm, user, HttpContext.RequestAborted);
        return granted.Contains(permission);
    }
    private async Task<(IActionResult? Error, int Farm, int User)> Scope()
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var user)) return (Unauthorized(), 0, 0);
        if (!int.TryParse(Request.Headers["FarmHouseId"].FirstOrDefault(), out var farm)) return (BadRequest(new { message = "A valid farmhouse is required." }), 0, 0);
        if (await farmhouses.GetByIdAsync(farm) is not { IsActive: true, IsDeleted: false }) return (NotFound(), 0, 0);
        globalAdministrator = await administration.IsGlobalAdministratorAsync(user);
        if (globalAdministrator) return (null, farm, user);
        var member = await memberships.GetMembershipAsync(user, farm);
        if (User.FindFirstValue(JwtClaimNames.FarmHouseId) != farm.ToString() ||
            member?.User is not { IsActive: true, IsDeleted: false } ||
            member.Role is not { IsActive: true, IsDeleted: false, RoleName: "Admin" or "Farm Owner" or "FarmOwner" }) return (Forbid(), 0, 0);
        return (null, farm, user);
    }
}
