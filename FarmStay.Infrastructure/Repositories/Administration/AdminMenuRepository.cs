using FarmStay.Application.DTOs.Administration;
using FarmStay.Application.Interfaces.Repositories;
using FarmStay.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FarmStay.Infrastructure.Repositories.Administration
{
    public class AdminMenuRepository : IAdminMenuRepository
    {
        private readonly AppDbContext _context;

        public AdminMenuRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<AdminMenuResponseDto> GetMenuAsync(
            int userId,
            int farmHouseId)
        {
            // Get current user's active membership and role
            var membership = await _context.UserMemberships
                .AsNoTracking()
                .Where(um =>
                    um.UserId == userId &&
                    um.FarmHouseId == farmHouseId &&
                    um.IsActive &&
                    !um.IsDeleted)
                .Join(
                    _context.Roles
                        .AsNoTracking()
                        .Where(r =>
                            r.IsActive &&
                            !r.IsDeleted),
                    um => um.RoleId,
                    r => r.RoleId,
                    (um, r) => new
                    {
                        um.RoleId,
                        RoleName = r.RoleName
                    })
                .FirstOrDefaultAsync();

            if (membership == null)
            {
                return new AdminMenuResponseDto();
            }

            var isSuperAdmin =
                string.Equals(
                    membership.RoleName,
                    "Super Admin",
                    StringComparison.OrdinalIgnoreCase);

            var result = new AdminMenuResponseDto();

            /*
             * SUPER ADMIN
             *
             * Super Admin can access all active/enabled modules
             * of the current FarmHouse.
             *
             * We do NOT depend on RoleId because RoleId can be
             * different for Super Admin in different FarmHouses.
             */
            if (isSuperAdmin)
            {
                var modules = await _context.farmHouseModules
                    .AsNoTracking()
                    .Where(fm =>
                        fm.FarmHouseId == farmHouseId &&
                        fm.IsEnabled &&
                        !fm.IsDeleted)
                    .Join(
                        _context.Modules
                            .AsNoTracking()
                            .Where(m => m.IsActive),
                        fm => fm.ModuleId,
                        m => m.ModuleId,
                        (fm, m) => m)
                    .OrderBy(m => m.DisplayOrder)
                    .ToListAsync();

                foreach (var moduleEntity in modules)
                {
                    var module = new AdminModuleMenuDto
                    {
                        ModuleId = moduleEntity.ModuleId,
                        ModuleName = moduleEntity.ModuleName,
                        DisplayName = moduleEntity.DisplayName ?? string.Empty,
                        Icon = moduleEntity.Icon ?? string.Empty,
                        Route = moduleEntity.Route ?? string.Empty,
                        DisplayOrder = moduleEntity.DisplayOrder
                    };

                    /*
                     * Super Admin:
                     * Get all active permissions which belong to
                     * this module through RoleModulePermissions.
                     *
                     * Permission table does not contain ModuleId,
                     * therefore RoleModulePermissions is used to
                     * identify module-wise permissions.
                     */
                    var permissions = await _context.RoleModulePermissions
                        .AsNoTracking()
                        .Where(rmp =>
                            rmp.IsAllowed &&
                            rmp.IsActive &&
                            !rmp.IsDeleted)
                        .Join(
                            _context.RoleModules
                                .AsNoTracking()
                                .Where(rm =>
                                    rm.ModuleId == moduleEntity.ModuleId &&
                                    rm.IsActive ),
                            rmp => rmp.RoleModuleId,
                            rm => rm.RoleModuleId,
                            (rmp, rm) => rmp.PermissionId)
                        .Join(
                            _context.Permissions
                                .AsNoTracking()
                                .Where(p => p.IsActive),
                            permissionId => permissionId,
                            p => p.PermissionId,
                            (permissionId, p) => p.PermissionName)
                        .Distinct()
                        .ToListAsync();

                    /*
                     * AdminMenu.Get is an API-level permission.
                     * It should not appear as a submenu.
                     */
                    var submenuGroups = permissions
                        .Where(p =>
                            p != "AdminMenu.Get" &&
                            p.Contains('.'))
                        .Select(p => p.Split('.')[0])
                        .Distinct()
                        .ToList();

                    foreach (var submenuName in submenuGroups)
                    {
                        var submenuPermissions = permissions
                            .Where(p =>
                                p != "AdminMenu.Get" &&
                                p.StartsWith(
                                    $"{submenuName}.",
                                    StringComparison.OrdinalIgnoreCase))
                            .ToList();

                        module.SubMenus.Add(new AdminSubMenuDto
                        {
                            Name = submenuName,
                            Route =
                                $"{module.Route.TrimEnd('/')}/" +
                                submenuName.ToLowerInvariant(),
                            Permissions = submenuPermissions
                        });
                    }

                    result.Modules.Add(module);
                }

                return result;
            }

            /*
             * NORMAL USERS
             *
             * Module access is determined by:
             *
             * UserMembership
             *      ↓
             * RoleModules
             *      ↓
             * Modules
             *      ↓
             * FarmHouseModules
             */
            var menuData = await _context.UserMemberships
                .AsNoTracking()
                .Where(um =>
                    um.UserId == userId &&
                    um.FarmHouseId == farmHouseId &&
                    um.IsActive &&
                    !um.IsDeleted)
                .Join(
                    _context.RoleModules
                        .AsNoTracking()
                        .Where(rm =>
                            rm.IsActive ),
                    um => um.RoleId,
                    rm => rm.RoleId,
                    (um, rm) => new
                    {
                        um.FarmHouseId,
                        rm.RoleModuleId,
                        rm.ModuleId
                    })
                .Join(
                    _context.Modules
                        .AsNoTracking()
                        .Where(m => m.IsActive),
                    x => x.ModuleId,
                    m => m.ModuleId,
                    (x, m) => new
                    {
                        x.FarmHouseId,
                        x.RoleModuleId,
                        Module = m
                    })
                .Join(
                    _context.farmHouseModules
                        .AsNoTracking()
                        .Where(fm =>
                            fm.IsEnabled &&
                            !fm.IsDeleted),
                    x => new
                    {
                        x.FarmHouseId,
                        x.Module.ModuleId
                    },
                    fm => new
                    {
                        fm.FarmHouseId,
                        fm.ModuleId
                    },
                    (x, fm) => x)
                .Select(x => new
                {
                    x.RoleModuleId,
                    x.Module
                })
                .Distinct()
                .OrderBy(x => x.Module.DisplayOrder)
                .ToListAsync();

            foreach (var item in menuData)
            {
                var module = new AdminModuleMenuDto
                {
                    ModuleId = item.Module.ModuleId,
                    ModuleName = item.Module.ModuleName,
                    DisplayName = item.Module.DisplayName ?? string.Empty,
                    Icon = item.Module.Icon ?? string.Empty,
                    Route = item.Module.Route ?? string.Empty,
                    DisplayOrder = item.Module.DisplayOrder
                };

                var permissions = await _context.RoleModulePermissions
                    .AsNoTracking()
                    .Where(rmp =>
                        rmp.RoleModuleId == item.RoleModuleId &&
                        rmp.IsAllowed &&
                        rmp.IsActive &&
                        !rmp.IsDeleted)
                    .Join(
                        _context.Permissions
                            .AsNoTracking()
                            .Where(p => p.IsActive),
                        rmp => rmp.PermissionId,
                        p => p.PermissionId,
                        (rmp, p) => p.PermissionName)
                    .ToListAsync();

                /*
                 * AdminMenu.Get is an API-level permission.
                 * It should NOT appear as a Dashboard submenu.
                 */
                var submenuGroups = permissions
                    .Where(p =>
                        p != "AdminMenu.Get" &&
                        p.Contains('.'))
                    .Select(p => p.Split('.')[0])
                    .Distinct()
                    .ToList();

                foreach (var submenuName in submenuGroups)
                {
                    var submenuPermissions = permissions
                        .Where(p =>
                            p != "AdminMenu.Get" &&
                            p.StartsWith(
                                $"{submenuName}.",
                                StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    module.SubMenus.Add(new AdminSubMenuDto
                    {
                        Name = submenuName,
                        Route =
                            $"{module.Route.TrimEnd('/')}/" +
                            submenuName.ToLowerInvariant(),
                        Permissions = submenuPermissions
                    });
                }

                result.Modules.Add(module);
            }

            return result;
        }
    }
}