using FarmStay.Application.Interfaces.Repositories;
using FarmStay.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FarmStay.Infrastructure.Repositories.Auth
{
    public class PermissionRepository : IPermissionRepository
    {
        private readonly AppDbContext _context;

        public PermissionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> HasPermissionAsync(
            int userId,
            int farmHouseId,
            string permissionName)
        {
            if (userId <= 0 ||
                farmHouseId <= 0 ||
                string.IsNullOrWhiteSpace(permissionName))
            {
                return false;
            }

            permissionName = permissionName.Trim();

            // =========================================================
            // SUPER ADMIN
            // =========================================================
            // Super Admin has full permission within the current
            // FarmHouse context.
            //
            // FarmHouseId is validated through UserMembership.
            // Therefore, Super Admin can access all permissions for
            // the current FarmHouse only.
            // =========================================================

            var isSuperAdmin = await _context.UserMemberships
                .AsNoTracking()
                .AnyAsync(um =>
                    um.UserId == userId &&
                    um.FarmHouseId == farmHouseId &&
                    um.RoleId == 1 &&
                    um.IsActive &&
                    !um.IsDeleted);

            if (isSuperAdmin)
            {
                return await _context.Permissions
                    .AsNoTracking()
                    .AnyAsync(p =>
                        p.PermissionName == permissionName &&
                        p.IsActive);
            }

            // =========================================================
            // NORMAL ROLE PERMISSION CHECK
            // =========================================================

            return await _context.UserMemberships
                .AsNoTracking()
                .Where(um =>
                    um.UserId == userId &&
                    um.FarmHouseId == farmHouseId &&
                    um.IsActive &&
                    !um.IsDeleted)
                .Join(
                    _context.RoleModules.AsNoTracking()
                        .Where(rm => rm.IsActive),
                    um => um.RoleId,
                    rm => rm.RoleId,
                    (um, rm) => new
                    {
                        um.FarmHouseId,
                        rm.RoleModuleId,
                        rm.ModuleId
                    })
                .Join(
                    _context.Modules.AsNoTracking()
                        .Where(m => m.IsActive),
                    rm => rm.ModuleId,
                    m => m.ModuleId,
                    (rm, m) => new
                    {
                        rm.FarmHouseId,
                        rm.RoleModuleId,
                        rm.ModuleId
                    })
                .Join(
                    _context.RoleModulePermissions.AsNoTracking()
                        .Where(rmp =>
                            rmp.IsActive &&
                            !rmp.IsDeleted &&
                            rmp.IsAllowed),
                    rm => rm.RoleModuleId,
                    rmp => rmp.RoleModuleId,
                    (rm, rmp) => new
                    {
                        rm.FarmHouseId,
                        rm.ModuleId,
                        rmp.PermissionId
                    })
                .Join(
                    _context.Permissions.AsNoTracking()
                        .Where(p =>
                            p.IsActive &&
                            p.PermissionName == permissionName),
                    rmp => rmp.PermissionId,
                    p => p.PermissionId,
                    (rmp, p) => new
                    {
                        rmp.FarmHouseId,
                        rmp.ModuleId
                    })
                .Join(
                    _context.farmHouseModules.AsNoTracking()
                        .Where(fm =>
                            fm.IsEnabled &&
                            !fm.IsDeleted),
                    rmp => new
                    {
                        rmp.FarmHouseId,
                        rmp.ModuleId
                    },
                    fm => new
                    {
                        fm.FarmHouseId,
                        fm.ModuleId
                    },
                    (rmp, fm) => rmp)
                .AnyAsync();
        }
    }
}