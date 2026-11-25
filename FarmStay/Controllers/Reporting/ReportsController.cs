using FarmStay.Application.Common.ApiResponse;
using FarmStay.Application.Reporting;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmStay.API.Controllers.Reporting;
[ApiController]
[Route("api/administration/reports")]
[Authorize(Roles = "Admin,Farm Owner,FarmOwner,Super Admin")]
public sealed class ReportsController(IReportingService reporting) : ControllerBase
{
    [HttpGet]
    public Task<IActionResult> Catalog(CancellationToken ct) => Respond(async () => {
        Response.Headers.CacheControl = "no-store";
        return Ok(new ApiResponse<IReadOnlyList<ReportCatalogEntry>> { Success = true,
            Message = "Available reports fetched successfully.", Data = await reporting.CatalogAsync(ct) });
    });

    [HttpGet("{report}")]
    public Task<IActionResult> List(string report, [FromQuery, CustomizeValidator(Skip = true)] ReportFilter filter, CancellationToken ct) => Respond(async () => {
        Response.Headers.CacheControl = "no-store";
        return Ok(new ApiResponse<object> { Success = true, Message = "Report fetched successfully.",
            Data = await reporting.ListAsync(report, filter, ct) });
    });

    [HttpGet("{report}/export/{format}")]
    public Task<IActionResult> Export(string report, string format, [FromQuery, CustomizeValidator(Skip = true)] ReportFilter filter, CancellationToken ct) => Respond(async () => {
        Response.Headers.CacheControl = "no-store";
        var file = await reporting.ExportAsync(report, filter, format, ct);
        return File(file.Content, file.ContentType, file.FileName);
    });

    private async Task<IActionResult> Respond(Func<Task<IActionResult>> action)
    {
        try { return await action(); }
        catch (ValidationException exception)
        {
            return BadRequest(new ApiResponse<object> { Message = string.Join(" ", exception.Errors.Select(e => e.ErrorMessage).Distinct()) });
        }
        catch (ReportException exception)
        {
            var status = exception.Error switch { ReportError.Unauthorized => 401, ReportError.Forbidden => 403,
                ReportError.NotFound => 404, ReportError.ExportLimit => 413, _ => 400 };
            return StatusCode(status, new ApiResponse<object> { Message = exception.Message });
        }
    }
}
