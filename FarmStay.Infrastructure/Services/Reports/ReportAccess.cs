using FarmStay.Application.Reporting;
using FarmStay.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FarmStay.Infrastructure.Reporting;
public sealed class ReportAccess(AppDbContext db, FarmStay.Application.Interfaces.Common.ICurrentTenantContext tenant) : IReportAccess
{
    public async Task<IReadOnlySet<string>> PermissionsAsync(int farmhouse, int user, CancellationToken ct)
    {
        var scope = await tenant.RequireAsync(ct);
        if (scope.FarmHouseId != farmhouse || scope.UserId != user)
            throw new FarmStay.Application.Interfaces.Common.TenantAccessException(403, "Report scope conflicts with your membership.");
        // One query for all report View/Export rights, rather than repeated per-report permission queries.
        var names = await (from membership in db.UserMemberships.AsNoTracking()
            join roleModule in db.RoleModules on membership.RoleId equals roleModule.RoleId
            join module in db.Modules on roleModule.ModuleId equals module.ModuleId
            join enabled in db.farmHouseModules on new { membership.FarmHouseId, module.ModuleId }
                equals new { enabled.FarmHouseId, enabled.ModuleId }
            join grant in db.RoleModulePermissions on roleModule.RoleModuleId equals grant.RoleModuleId
            join permission in db.Permissions on grant.PermissionId equals permission.PermissionId
            where membership.UserMembershipId == scope.MembershipId && membership.UserId == user && membership.FarmHouseId == farmhouse && membership.IsActive && !membership.IsDeleted &&
                membership.User!.IsActive && !membership.User.IsDeleted && membership.Role!.IsActive && !membership.Role.IsDeleted &&
                roleModule.IsActive && module.ModuleName == "Reporting" && module.IsActive && enabled.IsEnabled && !enabled.IsDeleted &&
                grant.IsActive && grant.IsAllowed && !grant.IsDeleted && permission.IsActive
            select permission.PermissionName).Distinct().ToListAsync(ct);
        return names.ToHashSet(StringComparer.Ordinal);
    }
}
