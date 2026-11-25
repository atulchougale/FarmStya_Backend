using FarmStay.Application.Accounts;
using FarmStay.Application.Common.ApiResponse;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace FarmStay.API.Controllers.Accounts;
[ApiController,Authorize,Route("api/administration/accounts")]
[RequestSizeLimit(6000000)]
[Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("Accounts")]
public sealed class AccountsController(IAccountsService service):ControllerBase
{
 [HttpGet("reports/{report}")] public Task<IActionResult> Report(string report,[FromQuery] AccountQuery q,CancellationToken ct)=>Respond(async()=>await service.ReportAsync(report,q,ct));
 [HttpGet("reports/{report}/export/{format}")] public async Task<IActionResult> Export(string report,string format,[FromQuery]AccountQuery q,CancellationToken ct) {try{var f=await service.ExportAsync(report,q,format,ct);return File(f.Content,f.ContentType,f.FileName);}catch(AccountsException e){return StatusCode(e.Status,new{success=false,message=e.Message});}}
 [HttpGet("vouchers/{id:long}")] public Task<IActionResult> Voucher(long id,CancellationToken ct)=>Respond(async()=>await service.VoucherAsync(id,ct));
 [HttpGet("{area}")] public Task<IActionResult> List(string area,[FromQuery]AccountQuery q,CancellationToken ct)=>Respond(async()=>await service.GetAsync(area,q,ct));
 [HttpPost("masters")] public Task<IActionResult> Master(MasterInput r,CancellationToken ct)=>Save("masters",null,r,ct);
 [HttpPut("masters/{id:long}")] public Task<IActionResult> Master(long id,MasterInput r,CancellationToken ct)=>Save("masters",id,r,ct);
 [HttpPost("ledgers")] public Task<IActionResult> Ledger(LedgerInput r,CancellationToken ct)=>Save("ledgers",null,r,ct);
 [HttpPut("ledgers/{id:long}")] public Task<IActionResult> Ledger(long id,LedgerInput r,CancellationToken ct)=>Save("ledgers",id,r,ct);
 [HttpPost("vendors")] public Task<IActionResult> Vendor(VendorInput r,CancellationToken ct)=>Save("vendors",null,r,ct);
 [HttpPut("vendors/{id:long}")] public Task<IActionResult> Vendor(long id,VendorInput r,CancellationToken ct)=>Save("vendors",id,r,ct);
 [HttpPost("money-accounts")] public Task<IActionResult> Money(MoneyAccountInput r,CancellationToken ct)=>Save("money-accounts",null,r,ct);
 [HttpPut("money-accounts/{id:long}")] public Task<IActionResult> Money(long id,MoneyAccountInput r,CancellationToken ct)=>Save("money-accounts",id,r,ct);
 [HttpPut("settings")] public Task<IActionResult> Settings(SettingsInput r,CancellationToken ct)=>Save("settings",null,r,ct);
 [HttpPut("periods/{id:long}")] public Task<IActionResult> Period(long id,ActionInput r,CancellationToken ct)=>Save("periods",id,r,ct);
 [HttpPost("vouchers")] public Task<IActionResult> Draft(VoucherInput r,CancellationToken ct)=>Save("vouchers",null,r,ct);
 [HttpPut("vouchers/{id:long}")] public Task<IActionResult> Draft(long id,VoucherInput r,CancellationToken ct)=>Save("vouchers",id,r,ct);
 [HttpPost("vouchers/{id:long}/post")] public Task<IActionResult> Post(long id,ActionInput r,CancellationToken ct)=>Respond(async()=>await service.PostAsync(id,r,false,ct));
 [HttpPost("vouchers/{id:long}/reverse")] public Task<IActionResult> Reverse(long id,ActionInput r,CancellationToken ct)=>Respond(async()=>await service.PostAsync(id,r,true,ct));
 [HttpPost("assets")] public Task<IActionResult> Asset(AssetInput r,CancellationToken ct)=>Save("assets",null,r,ct);
 [HttpPost("assets/{id:long}/{action}")] public Task<IActionResult> AssetAction(long id,string action,ActionInput r,CancellationToken ct)=>Respond(async()=>await service.AssetActionAsync(id,action,r,ct));
 [HttpPost("recurring")] public Task<IActionResult> Recurring(RecurringInput r,CancellationToken ct)=>Save("recurring",null,r,ct);
 [HttpPut("recurring/{id:long}")] public Task<IActionResult> Recurring(long id,RecurringInput r,CancellationToken ct)=>Save("recurring",id,r,ct);
 [HttpPost("statements")] public Task<IActionResult> Statement(StatementInput r,CancellationToken ct)=>Save("statements",null,r,ct);
 [HttpPost("statements/{id:long}/reconcile")] public Task<IActionResult> Match(long id,ActionInput r,CancellationToken ct)=>Respond(async()=>await service.ReconcileAsync(id,r,ct));
 [HttpPost("vouchers/{id:long}/attachments")] public Task<IActionResult> Upload(long id,IFormFile file,CancellationToken ct)=>Respond(async()=>{if(file.Length>5242880)throw new AccountsException(413,"Maximum 5 MB.");using var stream=new MemoryStream();await file.CopyToAsync(stream,ct);return await service.AttachmentAsync(id,file.FileName,file.ContentType,stream.ToArray(),ct);});
 [HttpGet("attachments/{id:long}")] public async Task<IActionResult> Attachment(long id,CancellationToken ct){try{var f=await service.DownloadAttachmentAsync(id,ct);Response.Headers["X-Content-Type-Options"]="nosniff";return File(f.Content,f.ContentType,f.FileName);}catch(AccountsException e){return StatusCode(e.Status,new{success=false,message=e.Message});}}
 Task<IActionResult> Save(string area,long? id,object request,CancellationToken ct)=>Respond(async()=>await service.SaveAsync(area,id,request,ct));
 async Task<IActionResult> Respond(Func<Task<object>> f){try{return Ok(new ApiResponse<object>{Success=true,Message="Accounts operation completed.",Data=await f()});}catch(AccountsException e){return StatusCode(e.Status,new{success=false,message=e.Message});}catch(ArgumentException e){return BadRequest(new{success=false,message=e.Message});}catch(DbUpdateConcurrencyException){return Conflict(new{success=false,message="Record changed. Refresh and try again."});}catch(DbUpdateException){return Conflict(new{success=false,message="A referenced record or unique code conflicts. Refresh and try again."});}}
}
