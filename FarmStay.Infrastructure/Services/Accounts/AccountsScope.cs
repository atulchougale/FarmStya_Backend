using System.Security.Claims;
using FarmStay.Application.Accounts;
using FarmStay.Application.Common.Constants;
using FarmStay.Application.Interfaces.Repositories;
using FarmStay.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
namespace FarmStay.Infrastructure.Accounts;
public sealed class AccountsScope(IHttpContextAccessor http,AppDbContext db):IAccountsScope
{
 public async Task<AccountScope> RequireAsync(string permission,CancellationToken ct)
 {
  var c=http.HttpContext;
  if(c?.User.Identity?.IsAuthenticated!=true||!int.TryParse(c.User.FindFirstValue(ClaimTypes.NameIdentifier),out var user)||!int.TryParse(c.User.FindFirstValue(JwtClaimNames.UserMembershipId),out var id)) throw new AccountsException(401,"An authenticated membership is required.");
  var m=await db.UserMemberships.AsNoTracking().Include(x=>x.User).Include(x=>x.Role).Include(x=>x.FarmHouse).SingleOrDefaultAsync(x=>x.UserMembershipId==id&&x.UserId==user&&x.IsActive&&!x.IsDeleted,ct);
  if(m?.User is not {IsActive:true,IsDeleted:false} || m.Role is not {IsActive:true,IsDeleted:false} || m.FarmHouse is not {IsActive:true,IsDeleted:false}) throw new AccountsException(403,"An active farmhouse membership is required.");
  var allowed = m.Role.RoleName == "Super Admin"
    ? (await db.Permissions.AsNoTracking().Where(x=>x.IsActive&&x.PermissionName.StartsWith("Accounts.")).Select(x=>x.PermissionName).ToListAsync(ct)).ToHashSet(StringComparer.Ordinal)
    : (await (from rm in db.RoleModules
        join mod in db.Modules on rm.ModuleId equals mod.ModuleId
        join fm in db.farmHouseModules on mod.ModuleId equals fm.ModuleId
        join grant in db.RoleModulePermissions on rm.RoleModuleId equals grant.RoleModuleId
        join right in db.Permissions on grant.PermissionId equals right.PermissionId
        where rm.RoleId==m.RoleId && rm.IsActive && mod.ModuleName=="Accounts" && mod.IsActive && fm.FarmHouseId==m.FarmHouseId && fm.IsEnabled && !fm.IsDeleted && grant.IsActive && grant.IsAllowed && !grant.IsDeleted && right.IsActive
        select right.PermissionName).Distinct().ToListAsync(ct)).ToHashSet(StringComparer.Ordinal);
  if(!allowed.Contains(permission)) throw new AccountsException(403,"You do not have this Accounts permission.");
  c.Response.Headers.CacheControl="no-store";
  return new(m.FarmHouseId,user,c.TraceIdentifier,allowed);
 }
}
