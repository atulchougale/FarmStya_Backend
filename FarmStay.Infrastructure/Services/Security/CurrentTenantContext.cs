using System.Security.Claims;
using FarmStay.Application.Interfaces.Common;
using FarmStay.Application.Common.Constants;
using FarmStay.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
namespace FarmStay.Infrastructure.Services.Security;
public sealed class CurrentTenantContext(IHttpContextAccessor accessor,AppDbContext db):ICurrentTenantContext,ICurrentUserContext
{
 VerifiedTenant? current;
 public async Task<VerifiedTenant> RequireAsync(CancellationToken ct=default){
  if(current!=null)return current;var c=accessor.HttpContext;
  if(c?.User.Identity?.IsAuthenticated!=true||!int.TryParse(c.User.FindFirstValue(ClaimTypes.NameIdentifier),out var user)||!int.TryParse(c.User.FindFirstValue(JwtClaimNames.UserMembershipId),out var membership))throw new TenantAccessException(401,"A valid authenticated membership is required.");
  var m=await db.UserMemberships.AsNoTracking().Include(x=>x.User).Include(x=>x.Role).Include(x=>x.FarmHouse).SingleOrDefaultAsync(x=>x.UserMembershipId==membership&&x.UserId==user&&x.IsActive&&!x.IsDeleted,ct);
  if(m?.User is not {IsActive:true,IsDeleted:false}||m.Role is not {IsActive:true,IsDeleted:false}||m.FarmHouse is not {IsActive:true,IsDeleted:false})throw new TenantAccessException(403,"An active membership is required.");
  if(c.User.FindAll(ClaimTypes.Role).Any(x=>x.Value!=m.Role.RoleName))throw new TenantAccessException(401,"Your authorization changed. Sign in again.");
  foreach(var value in new[]{c.Request.Headers["FarmHouseId"].FirstOrDefault(),c.User.FindFirstValue(JwtClaimNames.FarmHouseId)})if(value!=null&&(!int.TryParse(value,out var selected)||selected!=m.FarmHouseId))throw new TenantAccessException(403,"Farmhouse selection conflicts with your membership.");
  return current=new(m.FarmHouseId,user,membership,m.Role.RoleName,c.TraceIdentifier);
 }
 public async Task DemandAsync(params string[] permissions){var s=await RequireAsync();var enabled=from m in db.UserMemberships join rm in db.RoleModules on m.RoleId equals rm.RoleId join mod in db.Modules on rm.ModuleId equals mod.ModuleId join fm in db.farmHouseModules on mod.ModuleId equals fm.ModuleId join grant in db.RoleModulePermissions on rm.RoleModuleId equals grant.RoleModuleId join p in db.Permissions on grant.PermissionId equals p.PermissionId where m.UserMembershipId==s.MembershipId&&rm.IsActive&&mod.IsActive&&fm.FarmHouseId==s.FarmHouseId&&fm.IsEnabled&&!fm.IsDeleted&&grant.IsActive&&!grant.IsDeleted&&grant.IsAllowed&&p.IsActive&&permissions.Contains(p.PermissionName) select p.PermissionId;
  if(s.Role=="Super Admin"){if(await db.Permissions.AnyAsync(p=>p.IsActive&&permissions.Contains(p.PermissionName)))return;}
  else if(await enabled.AnyAsync())return;
  throw new TenantAccessException(403,"You do not have permission for this operation.");
 }
}
