using FarmStay.Application.Interfaces.Common;
using Microsoft.AspNetCore.Authorization;
namespace FarmStay.API.Middleware;
public sealed class TenantBoundaryMiddleware(RequestDelegate next)
{
 public async Task InvokeAsync(HttpContext c,ICurrentTenantContext tenant,IPublicTenantResolver publicTenant){
  var endpoint=c.GetEndpoint();if(endpoint!=null&&endpoint.Metadata.GetMetadata<IAllowAnonymous>()==null&&endpoint.Metadata.GetOrderedMetadata<IAuthorizeData>().Count>0){var scope=await tenant.RequireAsync(c.RequestAborted);
   foreach(var selector in new[]{c.Request.Query["FarmHouseId"].FirstOrDefault(),c.Request.RouteValues.TryGetValue("farmHouseId",out var route)?route?.ToString():null})
    if(selector!=null&&(!int.TryParse(selector,out var id)||id!=scope.FarmHouseId))throw new TenantAccessException(403,"Farmhouse selection conflicts with your membership.");
   if(c.Request.ContentType?.StartsWith("application/json",StringComparison.OrdinalIgnoreCase)==true&&c.Request.ContentLength!=0) {
    if(c.Request.ContentLength>6000000)throw new TenantAccessException(413,"JSON request is too large.");
    c.Request.EnableBuffering(30720,6000000);
    try {using var json=await System.Text.Json.JsonDocument.ParseAsync(c.Request.Body,cancellationToken:c.RequestAborted);CheckTenant(json.RootElement,scope.FarmHouseId);}
    catch(IOException){throw new TenantAccessException(413,"JSON request is too large.");}
    catch(System.Text.Json.JsonException){throw new TenantAccessException(400,"Invalid JSON request.");}
    finally{c.Request.Body.Position=0;}
   }
   c.Request.Headers["FarmHouseId"]=scope.FarmHouseId.ToString();}
  var action=endpoint?.Metadata.GetMetadata<Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor>();
  if(endpoint?.Metadata.GetMetadata<IAllowAnonymous>()!=null&&action?.ControllerName is "Public" or "PublicPage" or "Contact" or "Feedback" or "BookingCalendar" or "BookingPricing") {
    var farmhouse=await publicTenant.ResolveAsync(c.RequestAborted);c.Request.Headers["FarmHouseId"]=farmhouse.ToString();
  }
  c.Response.Headers["X-Content-Type-Options"]="nosniff";c.Response.Headers["Referrer-Policy"]="strict-origin-when-cross-origin";c.Response.Headers["X-Correlation-ID"]=c.TraceIdentifier;
  await next(c);
 }
 static void CheckTenant(System.Text.Json.JsonElement value,int tenant) {
  if(value.ValueKind==System.Text.Json.JsonValueKind.Object)foreach(var p in value.EnumerateObject()) {
   if(p.Name.Equals("farmHouseId",StringComparison.OrdinalIgnoreCase)&&(!int.TryParse(p.Value.ToString(),out var id)||id!=tenant))throw new TenantAccessException(403,"Farmhouse selection conflicts with your membership.");
   CheckTenant(p.Value,tenant);
  }
  else if(value.ValueKind==System.Text.Json.JsonValueKind.Array)foreach(var element in value.EnumerateArray())CheckTenant(element,tenant);
 }
}
