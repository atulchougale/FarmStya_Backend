using FarmStay.Application.Common.ApiResponse;
using FarmStay.Application.Interfaces.Services.Administration;
using FarmStay.Application.Reporting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace FarmStay.API.Controllers.Administration;
[ApiController]
[Route("api/administration/dashboard")]
[Authorize(Roles="Admin,Farm Owner,FarmOwner,Super Admin")]
public sealed class DashboardController(IDashboardService service) : ControllerBase
{
    [HttpGet] public Task<IActionResult> Get(CancellationToken ct) => Respond(async()=>await service.GetAsync(ct));
    [HttpGet("calendar")] public Task<IActionResult> Calendar(int year,int month,CancellationToken ct) => Respond(async()=>await service.CalendarAsync(year,month,ct));
    private async Task<IActionResult> Respond(Func<Task<object>> action)
    {
        Response.Headers.CacheControl="no-store";
        try{return Ok(new ApiResponse<object>{Success=true,Data=await action()});}
        catch(ReportException e){return StatusCode(e.Error switch{ReportError.Unauthorized=>401,ReportError.Forbidden=>403,ReportError.NotFound=>404,_=>400},new ApiResponse<object>{Message=e.Message});}
    }
}
